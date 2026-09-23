using System.Net;
using System.Net.Http.Json;
using System.Reflection;

using Application.Common.DTOs.Simulations.NightShift;
using Application.Simulations.NightShift;

using Domain.Common.Enums;
using Domain.Public.Users;

using FluentAssertions;

using Integration.Tests.Common;

using Microsoft.Extensions.DependencyInjection;

using Tests.Domain.Shared.TestDataCreator;

using Xunit;

namespace Integration.Tests.Simulations.NightShift;

public class NightShiftControllerTests : TestBase
{
    private readonly IntegrationContainerFixture _fixture;

    public NightShiftControllerTests(IntegrationContainerFixture fixture)
        : base(fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task Cases_AsStudent_ShouldListCuratedAndRandom()
    {
        var institute = await _instituteRepo.CreateInstituteAsync();
        var student = await _userRepo.CreateStudentAsync(institute.Guid);
        AuthenticateAs(student);

        var response = await _client.GetAsync(Routings.RestNightShiftCasesRouting);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await DeserializeResponseAsync<List<NightShiftCaseResponseDto>>(response);
        result!.Should().HaveCount(5);
        result[^1].Key.Should().Be("random");
        result[0].Name.Should().Be("Herr Keller, 58");
    }

    [Fact]
    public async Task Cases_WithEnglish_ShouldReturnEnglishNames()
    {
        var institute = await _instituteRepo.CreateInstituteAsync();
        var student = await _userRepo.CreateStudentAsync(institute.Guid);
        AuthenticateAs(student);

        var response = await _client.GetAsync(Routings.RestNightShiftCasesRouting + "?lang=en");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await DeserializeResponseAsync<List<NightShiftCaseResponseDto>>(response);
        result![0].Name.Should().Be("Mr Keller, 58");
    }

    [Fact]
    public async Task Start_WithKeller_ShouldReturnOpenerAndStammblatt()
    {
        var institute = await _instituteRepo.CreateInstituteAsync();
        var student = await _userRepo.CreateStudentAsync(institute.Guid);
        AuthenticateAs(student);

        var response = await StartAsync("keller");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await DeserializeResponseAsync<StartNightShiftSessionResponseDto>(response);
        result!.SessionId.Should().NotBeEmpty();
        result.FirstMessage.Should().StartWith("Seit zwei Stunden");
        result.Stammblatt.Pflegegrad.Should().Be("2");
        result.Language.Should().Be("de");
        result.Ended.Should().BeFalse();
    }

    [Fact]
    public async Task Chat_WithDo_ShouldStoreActionRole()
    {
        var institute = await _instituteRepo.CreateInstituteAsync();
        var student = await _userRepo.CreateStudentAsync(institute.Guid);
        AuthenticateAs(student);
        var start = await StartAndDeserializeAsync("keller");

        var chat = await _client.PostAsJsonAsync(
            $"{Routings.RestNightShiftChatRouting}/{start.SessionId}",
            new { text = "Ich kontrolliere die Infusion.", kind = "do" });

        chat.StatusCode.Should().Be(HttpStatusCode.OK);
        var detailResponse = await _client.GetAsync(
            $"{Routings.RestNightShiftSessionsRouting}/{start.SessionId}");
        var detail = await DeserializeResponseAsync<NightShiftSessionDetailResponseDto>(detailResponse);
        detail!.Messages.Should().Contain(message =>
            message.Role == "action" && message.Text.StartsWith("[Handlung] ", StringComparison.Ordinal));
    }

    [Fact]
    public async Task Chat_AfterEnde_ShouldReturnSessionAlreadyEnded()
    {
        var institute = await _instituteRepo.CreateInstituteAsync();
        var student = await _userRepo.CreateStudentAsync(institute.Guid);
        AuthenticateAs(student);
        var start = await StartAndDeserializeAsync("keller");

        var firstChat = await _client.PostAsJsonAsync(
            $"{Routings.RestNightShiftChatRouting}/{start.SessionId}",
            new { text = "Tschuess", kind = "say" });
        var secondChat = await _client.PostAsJsonAsync(
            $"{Routings.RestNightShiftChatRouting}/{start.SessionId}",
            new { text = "Hallo", kind = "say" });

        var firstResult = await DeserializeResponseAsync<NightShiftMessageResponseDto>(firstChat);
        firstResult!.Ended.Should().BeTrue();
        secondChat.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var error = await DeserializeResponseAsync<ErrorResponseDto>(secondChat);
        error!.ErrorCode.Should().Be((int)NightShiftErrorCodes.SessionAlreadyEnded);
    }

    [Fact]
    public async Task Chat_AsOtherUser_ShouldReturnForbiddenCode()
    {
        var institute = await _instituteRepo.CreateInstituteAsync();
        var student = await _userRepo.CreateStudentAsync(institute.Guid);
        var otherStudent = await _userRepo.CreateStudentAsync(institute.Guid);
        AuthenticateAs(student);
        var start = await StartAndDeserializeAsync("keller");
        AuthenticateAs(otherStudent);

        var response = await _client.PostAsJsonAsync(
            $"{Routings.RestNightShiftChatRouting}/{start.SessionId}",
            new { text = "Hallo", kind = "say" });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var error = await DeserializeResponseAsync<ErrorResponseDto>(response);
        error!.ErrorCode.Should().Be((int)NightShiftErrorCodes.Forbidden);
    }

    [Fact]
    public async Task Finish_Twice_ShouldCallLlmOnce()
    {
        var institute = await _instituteRepo.CreateInstituteAsync();
        var student = await _userRepo.CreateStudentAsync(institute.Guid);
        AuthenticateAs(student);
        var start = await StartAndDeserializeAsync("keller");
        await _client.PostAsJsonAsync(
            $"{Routings.RestNightShiftChatRouting}/{start.SessionId}",
            new { text = "Ich helfe Ihnen gleich.", kind = "say" });
        var fakeLlmClient = _fixture.Factory.Services.GetRequiredService<FakeLlmClient>();
        var callsBeforeFinish = fakeLlmClient.CallCount;

        var firstResponse = await _client.PostAsJsonAsync(
            $"{Routings.RestNightShiftFinishRouting}/{start.SessionId}",
            new { });
        var secondResponse = await _client.PostAsJsonAsync(
            $"{Routings.RestNightShiftFinishRouting}/{start.SessionId}",
            new { });

        var firstResult = await DeserializeResponseAsync<NightShiftFeedbackResponseDto>(firstResponse);
        var secondResult = await DeserializeResponseAsync<NightShiftFeedbackResponseDto>(secondResponse);
        firstResult!.Summary.Should().Be(secondResult!.Summary);
        fakeLlmClient.CallCount.Should().Be(callsBeforeFinish + 1);
    }

    [Fact]
    public async Task Finish_WithoutStudentTurn_ShouldReturnNotOk()
    {
        var institute = await _instituteRepo.CreateInstituteAsync();
        var student = await _userRepo.CreateStudentAsync(institute.Guid);
        AuthenticateAs(student);
        var start = await StartAndDeserializeAsync("keller");

        var response = await _client.PostAsJsonAsync(
            $"{Routings.RestNightShiftFinishRouting}/{start.SessionId}",
            new { });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await DeserializeResponseAsync<NightShiftFeedbackResponseDto>(response);
        result!.Ok.Should().BeFalse();
        result.Summary.Should().Contain("Noch kein Schuelerbeitrag");
    }

    [Fact]
    public async Task Sessions_ShouldListOnlyOwn()
    {
        var institute = await _instituteRepo.CreateInstituteAsync();
        var student = await _userRepo.CreateStudentAsync(institute.Guid);
        var otherStudent = await _userRepo.CreateStudentAsync(institute.Guid);
        AuthenticateAs(student);
        await StartAsync("keller");
        await StartAsync("schmidt");
        AuthenticateAs(otherStudent);
        await StartAsync("yilmaz");
        AuthenticateAs(student);

        var response = await _client.GetAsync(Routings.RestNightShiftSessionsRouting);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await DeserializeResponseAsync<List<NightShiftSessionSummaryResponseDto>>(response);
        result!.Should().HaveCount(2);
    }

    [Fact]
    public async Task Start_AsServer_ShouldBeRejected()
    {
        var server = UserTestData.CreateUser(Roles.Student);
        var role = typeof(UserEntity).GetProperty("Role", BindingFlags.Instance | BindingFlags.Public);
        role!.SetValue(server, Roles.Server);
        AuthenticateAs(server);

        var response = await StartAsync("keller");

        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.Unauthorized,
            HttpStatusCode.Forbidden,
            HttpStatusCode.BadRequest);
        if (response.StatusCode == HttpStatusCode.BadRequest)
        {
            var error = await DeserializeResponseAsync<ErrorResponseDto>(response);
            error!.ErrorCode.Should().Be((int)NightShiftErrorCodes.Forbidden);
        }
    }

    private Task<HttpResponseMessage> StartAsync(string caseKey)
    {
        return _client.PostAsJsonAsync(
            Routings.RestNightShiftStartRouting,
            new { caseKey });
    }

    private async Task<StartNightShiftSessionResponseDto> StartAndDeserializeAsync(string caseKey)
    {
        var response = await StartAsync(caseKey);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        return (await DeserializeResponseAsync<StartNightShiftSessionResponseDto>(response))!;
    }
}
