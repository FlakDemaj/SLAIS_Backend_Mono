using System.Net;
using System.Net.Http.Json;
using System.Reflection;
using System.Text.Json;

using Application.Common.DTOs.Simulations.NightShift;
using Application.Common.Interfaces.Services;
using Application.Simulations.NightShift;
using Application.Simulations.NightShift.Cases;

using Domain.Common.Enums;
using Domain.Public.Users;

using FluentAssertions;

using Integration.Tests.Common;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using Tests.Domain.Shared.Builders;

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
        await _nightShiftTemplateRepo.SeedCuratedAsync();
        var institute = await _instituteRepo.CreateInstituteAsync();
        var student = await _userRepo.CreateStudentAsync(institute.Guid);
        AuthenticateAs(student);

        var response = await _client.GetAsync(Routings.RestNightShiftCasesRouting);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await DeserializeResponseAsync<List<NightShiftCaseResponseDto>>(response);
        result!.Should().HaveCount(6);
        result[^1].Key.Should().Be("random");
        result[0].Name.Should().Be("Herr Keller, 58");
    }

    [Fact]
    public async Task Cases_WithEnglish_ShouldReturnEnglishNames()
    {
        await _nightShiftTemplateRepo.SeedCuratedAsync();
        var institute = await _instituteRepo.CreateInstituteAsync();
        var student = await _userRepo.CreateStudentAsync(institute.Guid);
        AuthenticateAs(student);

        var response = await _client.GetAsync(Routings.RestNightShiftCasesRouting + "?lang=en");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await DeserializeResponseAsync<List<NightShiftCaseResponseDto>>(response);
        result![0].Name.Should().Be("Mr Keller, 58");
    }

    [Fact]
    public async Task Cases_ShouldNotListPendingOrArchivedTemplates()
    {
        await _nightShiftTemplateRepo.SeedCuratedAsync();
        var pendingTemplate = new NightShiftTemplateEntityBuilder()
            .WithKey("entwurf")
            .Build();
        var archivedTemplate = new NightShiftTemplateEntityBuilder()
            .WithKey("archiv")
            .WithEnglishText()
            .WithState(States.Active)
            .Build();
        archivedTemplate.Archive(null);
        await _nightShiftTemplateRepo.CreateAsync(pendingTemplate);
        await _nightShiftTemplateRepo.CreateAsync(archivedTemplate);
        var institute = await _instituteRepo.CreateInstituteAsync();
        var student = await _userRepo.CreateStudentAsync(institute.Guid);
        AuthenticateAs(student);

        var response = await _client.GetAsync(Routings.RestNightShiftCasesRouting);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await DeserializeResponseAsync<List<NightShiftCaseResponseDto>>(response);
        result!.Should().NotContain(template => template.Key == "entwurf");
        result.Should().NotContain(template => template.Key == "archiv");
    }

    [Fact]
    public async Task Cases_ShouldOrderBySortOrder()
    {
        await _nightShiftTemplateRepo.SeedCuratedAsync();
        var template = new NightShiftTemplateEntityBuilder()
            .WithKey("erster")
            .WithSortOrder(1)
            .WithEnglishText()
            .WithState(States.Active)
            .Build();
        await _nightShiftTemplateRepo.CreateAsync(template);
        var institute = await _instituteRepo.CreateInstituteAsync();
        var student = await _userRepo.CreateStudentAsync(institute.Guid);
        AuthenticateAs(student);

        var response = await _client.GetAsync(Routings.RestNightShiftCasesRouting);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await DeserializeResponseAsync<List<NightShiftCaseResponseDto>>(response);
        result![0].Key.Should().Be("erster");
    }

    [Fact]
    public async Task Cases_WithEnglish_WhenEnglishTextMissing_ShouldFallbackToGermanAndFlagIt()
    {
        var template = new NightShiftTemplateEntityBuilder()
            .WithKey("nur-deutsch")
            .WithEnglishText()
            .WithState(States.Active)
            .Build();
        template.Texts.Remove(template.GetText(Language.English)!);
        await _nightShiftTemplateRepo.CreateAsync(template);
        var institute = await _instituteRepo.CreateInstituteAsync();
        var student = await _userRepo.CreateStudentAsync(institute.Guid);
        AuthenticateAs(student);

        var response = await _client.GetAsync(Routings.RestNightShiftCasesRouting + "?lang=en");
        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        document.RootElement[0].GetProperty("name").GetString().Should().Be("Herr Keller, 58");
        document.RootElement[0].GetProperty("languageFallback").GetBoolean().Should().BeTrue();
    }

    [Fact]
    public async Task Start_WithKeller_ShouldReturnOpenerAndStammblatt()
    {
        await _nightShiftTemplateRepo.SeedCuratedAsync();
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
    public async Task Start_WithArchivedKey_ShouldReturnInvalidCase()
    {
        var template = new NightShiftTemplateEntityBuilder()
            .WithKey("archiv")
            .WithEnglishText()
            .WithState(States.Active)
            .Build();
        template.Archive(null);
        await _nightShiftTemplateRepo.CreateAsync(template);
        var institute = await _instituteRepo.CreateInstituteAsync();
        var student = await _userRepo.CreateStudentAsync(institute.Guid);
        AuthenticateAs(student);

        var response = await StartAsync("archiv");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var error = await DeserializeResponseAsync<ErrorResponseDto>(response);
        error!.ErrorCode.Should().Be((int)NightShiftErrorCodes.InvalidCase);
    }

    [Fact]
    public async Task Start_WithoutActiveTemplates_ShouldReturnNoActiveTemplates()
    {
        var institute = await _instituteRepo.CreateInstituteAsync();
        var student = await _userRepo.CreateStudentAsync(institute.Guid);
        AuthenticateAs(student);

        var response = await _client.PostAsJsonAsync(Routings.RestNightShiftStartRouting, new { });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var error = await DeserializeResponseAsync<ErrorResponseDto>(response);
        error!.ErrorCode.Should().Be((int)NightShiftErrorCodes.NoActiveTemplates);
    }

    [Fact]
    public async Task Start_ShouldStoreTemplateGuidOnSession()
    {
        var templates = await _nightShiftTemplateRepo.SeedCuratedAsync();
        var kellerTemplate = templates.Single(template => template.Key == "keller");
        var institute = await _instituteRepo.CreateInstituteAsync();
        var student = await _userRepo.CreateStudentAsync(institute.Guid);
        AuthenticateAs(student);
        await StartAsync("keller");

        var response = await _client.GetAsync(Routings.RestNightShiftSessionsRouting);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await DeserializeResponseAsync<List<NightShiftSessionSummaryResponseDto>>(response);
        result![0].TemplateId.Should().Be(kellerTemplate.Guid);
    }

    [Fact]
    public async Task Chat_WithDo_ShouldStoreActionRole()
    {
        await _nightShiftTemplateRepo.SeedCuratedAsync();
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
    public async Task Session_Detail_ShouldContainSessionCaseStammblattMessagesFeedback()
    {
        await _nightShiftTemplateRepo.SeedCuratedAsync();
        var institute = await _instituteRepo.CreateInstituteAsync();
        var student = await _userRepo.CreateStudentAsync(institute.Guid);
        AuthenticateAs(student);
        var start = await StartAndDeserializeAsync("keller");

        var response = await _client.GetAsync(
            $"{Routings.RestNightShiftSessionsRouting}/{start.SessionId}");
        var content = await response.Content.ReadAsStringAsync();
        using var document = JsonDocument.Parse(content);

        document.RootElement.TryGetProperty("session", out _).Should().BeTrue();
        document.RootElement.TryGetProperty("case", out _).Should().BeTrue();
        document.RootElement.TryGetProperty("stammblatt", out _).Should().BeTrue();
        document.RootElement.TryGetProperty("messages", out _).Should().BeTrue();
        document.RootElement.TryGetProperty("feedback", out _).Should().BeTrue();
    }

    [Fact]
    public async Task Chat_AfterEnde_ShouldReturnSessionAlreadyEnded()
    {
        await _nightShiftTemplateRepo.SeedCuratedAsync();
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
        await _nightShiftTemplateRepo.SeedCuratedAsync();
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
        await _nightShiftTemplateRepo.SeedCuratedAsync();
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
        await _nightShiftTemplateRepo.SeedCuratedAsync();
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

        var chat = await _client.PostAsJsonAsync(
            $"{Routings.RestNightShiftChatRouting}/{start.SessionId}",
            new { text = "Ich bin jetzt bei Ihnen.", kind = "say" });
        chat.StatusCode.Should().Be(HttpStatusCode.OK);
        var detailResponse = await _client.GetAsync(
            $"{Routings.RestNightShiftSessionsRouting}/{start.SessionId}");
        using var detail = JsonDocument.Parse(await detailResponse.Content.ReadAsStringAsync());
        detail.RootElement.GetProperty("feedback").ValueKind.Should().Be(JsonValueKind.Null);
    }

    [Fact]
    public async Task Finish_WithMalformedFeedback_ShouldNotPersist()
    {
        await _nightShiftTemplateRepo.SeedCuratedAsync();
        var institute = await _instituteRepo.CreateInstituteAsync();
        var student = await _userRepo.CreateStudentAsync(institute.Guid);
        AuthenticateAs(student);
        var start = await StartAndDeserializeAsync("keller");
        await _client.PostAsJsonAsync(
            $"{Routings.RestNightShiftChatRouting}/{start.SessionId}",
            new { text = "Ich hoere Ihnen zu.", kind = "say" });
        var fakeLlmClient = _fixture.Factory.Services.GetRequiredService<FakeLlmClient>();
        var callsBeforeFinish = fakeLlmClient.CallCount;
        fakeLlmClient.FeedbackMode = "malformed";

        try
        {
            var firstResponse = await _client.PostAsJsonAsync(
                $"{Routings.RestNightShiftFinishRouting}/{start.SessionId}",
                new { });
            var secondResponse = await _client.PostAsJsonAsync(
                $"{Routings.RestNightShiftFinishRouting}/{start.SessionId}",
                new { });

            var firstResult = await DeserializeResponseAsync<NightShiftFeedbackResponseDto>(firstResponse);
            var secondResult = await DeserializeResponseAsync<NightShiftFeedbackResponseDto>(secondResponse);
            firstResult!.Ok.Should().BeFalse();
            secondResult!.Ok.Should().BeFalse();
            fakeLlmClient.CallCount.Should().Be(callsBeforeFinish + 2);
        }
        finally
        {
            fakeLlmClient.FeedbackMode = "valid";
        }
    }

    [Fact]
    public void Prompts_ShouldContainNoLevelAndNoUnreplacedPlaceholders()
    {
        var promptBuilder = _fixture.Factory.Services.GetRequiredService<INightShiftPromptBuilder>();

        foreach (var language in new[] { Language.German, Language.English })
        {
            var template = NightShiftTemplateTestData.CreateCuratedTemplates()[0];
            var patientCase = PatientCaseFactory.FromTemplate(template, language).Case;
            var prompts = new[]
            {
                promptBuilder.BuildPatientPrompt(patientCase, language),
                promptBuilder.BuildFeedbackPrompt(patientCase, language),
                promptBuilder.BuildGeneratorPrompt(language)
            };

            foreach (var prompt in prompts)
            {
                prompt.Should().NotContain("{{");
                prompt.Should().NotContain("{{LEVEL}}");
                prompt.Should().NotContainEquivalentOf("Schwierigkeitsgrad");
                prompt.Should().NotContainEquivalentOf("level:");
                prompt.Should().NotContainEquivalentOf("level (");
            }
        }
    }

    [Fact]
    public async Task Sessions_ShouldListOnlyOwn()
    {
        await _nightShiftTemplateRepo.SeedCuratedAsync();
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
        await _nightShiftTemplateRepo.SeedCuratedAsync();
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

    [Fact]
    public async Task Migration_ShouldCreateTemplateTables()
    {
        var tables = await _dbContext.Database.SqlQueryRaw<string>(
            """
            select table_name as "Value"
            from information_schema.tables
            where table_schema = 'simulation'
              and table_name in ('night_shift_templates', 'night_shift_template_texts')
            """)
            .ToListAsync();
        var columns = await _dbContext.Database.SqlQueryRaw<string>(
            """
            select column_name as "Value"
            from information_schema.columns
            where table_schema = 'simulation'
              and table_name = 'night_shift_sessions'
              and column_name = 'fk_night_shift_template_guid'
            """)
            .ToListAsync();

        tables.Should().Contain("night_shift_templates");
        tables.Should().Contain("night_shift_template_texts");
        columns.Should().Contain("fk_night_shift_template_guid");
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
