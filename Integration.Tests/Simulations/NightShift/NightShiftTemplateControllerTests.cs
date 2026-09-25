using System.Net;
using System.Net.Http.Json;

using Application.Common.DTOs.Base;
using Application.Common.DTOs.Simulations.NightShift;
using Application.Simulations.NightShift;

using FluentAssertions;

using Integration.Tests.Common;

using Microsoft.Extensions.DependencyInjection;

using Xunit;

namespace Integration.Tests.Simulations.NightShift;

public class NightShiftTemplateControllerTests : TestBase
{
    private readonly IntegrationContainerFixture _fixture;

    public NightShiftTemplateControllerTests(IntegrationContainerFixture fixture)
        : base(fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task Create_AsAdmin_ShouldReturnPendingTemplate()
    {
        var admin = await CreateAndAuthenticateAdminAsync();

        var response = await CreateAsync("neu-1", GermanText());

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var templates = await GetTemplatesAsync();
        var template = templates.Single(item => item.Key == "neu-1");
        template.State.Should().Be("pending");
        template.Version.Should().Be(2);
    }

    [Fact]
    public async Task Create_AsStudent_ShouldReturnForbidden()
    {
        var institute = await _instituteRepo.CreateInstituteAsync();
        var student = await _userRepo.CreateStudentAsync(institute.Guid);
        AuthenticateAs(student);

        var response = await CreateAsync("neu-1", GermanText());

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Create_WithExistingKey_ShouldReturnKeyAlreadyExists()
    {
        await _nightShiftTemplateRepo.SeedCuratedAsync();
        await CreateAndAuthenticateAdminAsync();

        var response = await CreateAsync("keller", GermanText());

        await AssertErrorAsync(response, -520011);
    }

    [Fact]
    public async Task Update_WithStaleVersion_ShouldReturnModifiedByOtherUser()
    {
        await CreateAndAuthenticateAdminAsync();
        var template = await CreateAndGetAsync("neu-1", GermanText());

        var response = await _client.PutAsJsonAsync(
            $"{Routings.RestNightShiftTemplateRouting}/{template.TemplateId}",
            new
            {
                expectedVersion = 99,
                key = template.Key,
                tensionStart = template.TensionStart,
                sortOrder = template.SortOrder
            });

        await AssertErrorAsync(response, -520009);
    }

    [Fact]
    public async Task Update_ShouldReplaceEnglishText()
    {
        await CreateAndAuthenticateAdminAsync();
        var template = await CreateAndGetAsync("neu-1", GermanText());

        var response = await _client.PutAsJsonAsync(
            $"{Routings.RestNightShiftTemplateRouting}/{template.TemplateId}",
            new
            {
                expectedVersion = template.Version,
                key = template.Key,
                tensionStart = template.TensionStart,
                sortOrder = template.SortOrder,
                english = EnglishText()
            });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await DeserializeResponseAsync<GetNightShiftTemplateResponseDto>(response);
        result!.Texts["en"].Name.Should().Be("Mr Keller, 58");
    }

    [Fact]
    public async Task SetState_ActiveWithoutEnglish_ShouldReturnActivationNeedsBothLanguages()
    {
        await CreateAndAuthenticateAdminAsync();
        var template = await CreateAndGetAsync("neu-1", GermanText());

        var response = await SetStateAsync(template.TemplateId, template.Version, "active");

        await AssertErrorAsync(response, -530009);
    }

    [Fact]
    public async Task SetState_Active_ShouldListInStudentCases()
    {
        var admin = await CreateAndAuthenticateAdminAsync();
        var template = await CreateAndGetAsync("neu-1", GermanText(), EnglishText());
        var stateResponse = await SetStateAsync(template.TemplateId, template.Version, "active");
        stateResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var student = await _userRepo.CreateStudentAsync(admin.InstituteGuid);
        AuthenticateAs(student);

        var response = await _client.GetAsync(Routings.RestNightShiftCasesRouting);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var cases = await DeserializeResponseAsync<List<NightShiftCaseResponseDto>>(response);
        cases!.Should().Contain(item => item.Key == "neu-1");
    }

    [Fact]
    public async Task Delete_ShouldHideFromListAndStudentCases()
    {
        var templates = await _nightShiftTemplateRepo.SeedCuratedAsync();
        var keller = templates.Single(template => template.Key == "keller");
        var admin = await CreateAndAuthenticateAdminAsync();

        var response = await _client.DeleteAsync(
            $"{Routings.RestNightShiftTemplateRouting}/{keller.Guid}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var remainingTemplates = await GetTemplatesAsync();
        remainingTemplates.Should().NotContain(item => item.Key == "keller");
        var student = await _userRepo.CreateStudentAsync(admin.InstituteGuid);
        AuthenticateAs(student);
        var casesResponse = await _client.GetAsync(Routings.RestNightShiftCasesRouting);
        var cases = await DeserializeResponseAsync<List<NightShiftCaseResponseDto>>(casesResponse);
        cases!.Should().NotContain(item => item.Key == "keller");
    }

    [Fact]
    public async Task Translate_ShouldReturnEnglishText()
    {
        await CreateAndAuthenticateAdminAsync();

        var response = await _client.PostAsJsonAsync(
            Routings.RestNightShiftTemplateTranslateRouting,
            new { sourceLanguage = "de", targetLanguage = "en", text = GermanText() });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await DeserializeResponseAsync<NightShiftTemplateTextDto>(response);
        result!.Name.Should().Be("Mr Keller, 58");
    }

    [Fact]
    public async Task Translate_WhenLlmFails_ShouldReturnTranslationUnavailable()
    {
        await CreateAndAuthenticateAdminAsync();
        var fakeLlmClient = _fixture.Factory.Services.GetRequiredService<FakeLlmClient>();
        fakeLlmClient.ThrowUnavailable = true;

        try
        {
            var response = await _client.PostAsJsonAsync(
                Routings.RestNightShiftTemplateTranslateRouting,
                new { sourceLanguage = "de", targetLanguage = "en", text = GermanText() });

            await AssertErrorAsync(response, -520010);
        }
        finally
        {
            fakeLlmClient.ThrowUnavailable = false;
        }
    }

    [Fact]
    public async Task FromSession_OnRandomSession_ShouldCreateDraft()
    {
        var institute = await _instituteRepo.CreateInstituteAsync();
        var student = await _userRepo.CreateStudentAsync(institute.Guid);
        AuthenticateAs(student);
        var startResponse = await StartAsync(new { caseKey = "random" });
        var start = await DeserializeResponseAsync<StartNightShiftSessionResponseDto>(startResponse);
        var admin = await _userRepo.CreateAdminAsync(institute.Guid);
        AuthenticateAs(admin);

        var response = await _client.PostAsJsonAsync(
            $"{Routings.RestNightShiftTemplateFromSessionRouting}/{start!.SessionId}",
            new { key = "aus-session" });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var templates = await GetTemplatesAsync();
        var template = templates.Single(item => item.Key == "aus-session");
        template.State.Should().Be("pending");
        template.Texts.Should().ContainKey("de");
    }

    [Fact]
    public async Task FromSession_OnTemplateSession_ShouldReturnSessionIsNotRandomCase()
    {
        await _nightShiftTemplateRepo.SeedCuratedAsync();
        var institute = await _instituteRepo.CreateInstituteAsync();
        var student = await _userRepo.CreateStudentAsync(institute.Guid);
        AuthenticateAs(student);
        var startResponse = await StartAsync(new { caseKey = "keller" });
        var start = await DeserializeResponseAsync<StartNightShiftSessionResponseDto>(startResponse);
        var admin = await _userRepo.CreateAdminAsync(institute.Guid);
        AuthenticateAs(admin);

        var response = await _client.PostAsJsonAsync(
            $"{Routings.RestNightShiftTemplateFromSessionRouting}/{start!.SessionId}",
            new { key = "aus-session" });

        await AssertErrorAsync(response, -520013);
    }

    [Fact]
    public async Task Start_PendingTemplate_AsAdmin_ShouldWork()
    {
        await CreateAndAuthenticateAdminAsync();
        var template = await CreateAndGetAsync("neu-1", GermanText(), EnglishText());

        var response = await StartAsync(new { templateId = template.TemplateId });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Start_PendingTemplate_AsStudent_ShouldReturnTemplateNotActive()
    {
        var admin = await CreateAndAuthenticateAdminAsync();
        var template = await CreateAndGetAsync("neu-1", GermanText(), EnglishText());
        var student = await _userRepo.CreateStudentAsync(admin.InstituteGuid);
        AuthenticateAs(student);

        var response = await StartAsync(new { templateId = template.TemplateId });

        await AssertErrorAsync(response, -520014);
    }

    private async Task<dynamic> CreateAndAuthenticateAdminAsync()
    {
        var institute = await _instituteRepo.CreateInstituteAsync();
        var admin = await _userRepo.CreateAdminAsync(institute.Guid);
        AuthenticateAs(admin);
        return admin;
    }

    private async Task<GetNightShiftTemplateResponseDto> CreateAndGetAsync(
        string key,
        object german,
        object? english = null)
    {
        var response = await CreateAsync(key, german, english);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var templates = await GetTemplatesAsync();
        return templates.Single(item => item.Key == key);
    }

    private Task<HttpResponseMessage> CreateAsync(
        string key,
        object german,
        object? english = null)
    {
        return _client.PostAsJsonAsync(
            Routings.RestNightShiftTemplateRouting,
            new { key, tensionStart = 5, sortOrder = 60, german, english });
    }

    private async Task<List<GetNightShiftTemplateResponseDto>> GetTemplatesAsync()
    {
        var response = await _client.GetAsync(Routings.RestNightShiftTemplateRouting);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        return (await DeserializeResponseAsync<List<GetNightShiftTemplateResponseDto>>(response))!;
    }

    private Task<HttpResponseMessage> SetStateAsync(
        Guid templateId,
        int expectedVersion,
        string state)
    {
        return _client.PostAsJsonAsync(
            $"{Routings.RestNightShiftTemplateRouting}/{templateId}/state",
            new { expectedVersion, state });
    }

    private Task<HttpResponseMessage> StartAsync(object request)
    {
        return _client.PostAsJsonAsync(Routings.RestNightShiftStartRouting, request);
    }

    private static async Task AssertErrorAsync(
        HttpResponseMessage response,
        int errorCode)
    {
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var error = await DeserializeResponseAsync<ErrorResponseDto>(response);
        error!.ErrorCode.Should().Be(errorCode);
    }

    private static object GermanText()
    {
        return new
        {
            name = "Herr Keller, 58",
            situation = "Akute Schmerzkrise nach OP, fuehlt sich nicht ernst genommen.",
            emotion = "wuetend, verzweifelt, misstrauisch",
            learningGoal = "Deeskalation und Schmerz ernst nehmen.",
            opener = "Seit zwei Stunden klingle ich und keiner kommt.",
            born = "14.03.1968",
            gender = "maennlich",
            admission = "Station Chirurgie, post-OP",
            diagnoses = "Z.n. Hueft-TEP rechts; arterielle Hypertonie",
            allergies = "Penicillin",
            medication = "Analgesie nach Schema; Heparin",
            careLevel = "2",
            risks = "Sturzrisiko erhoeht; Schmerzexazerbation",
            resuscitation = "Ja, keine Patientenverfuegung",
            relatives = "Ehefrau hinterlegt"
        };
    }

    private static object EnglishText()
    {
        return new
        {
            name = "Mr Keller, 58",
            situation = "Acute pain after surgery and feels ignored.",
            emotion = "angry, desperate and distrustful",
            learningGoal = "De-escalate and take the pain seriously.",
            opener = "I have been ringing for two hours and nobody comes.",
            born = "14/03/1968",
            gender = "male",
            admission = "Surgical ward, post-operative",
            diagnoses = "Status after hip replacement; hypertension",
            allergies = "Penicillin",
            medication = "Analgesia according to plan; heparin",
            careLevel = "2",
            risks = "Increased fall risk; pain exacerbation",
            resuscitation = "Yes, no advance directive",
            relatives = "Wife is registered"
        };
    }
}
