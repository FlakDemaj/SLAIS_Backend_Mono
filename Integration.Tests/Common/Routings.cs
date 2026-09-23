namespace Integration.Tests.Common;

internal static class Routings
{
    private const string BaseRestString = "/rest/";

    internal const string RestAuthenticationRouting = BaseRestString + "Authentication/";

    internal const string RestUserRouting = BaseRestString + "User";

    internal const string RestNightShiftCasesRouting = BaseRestString + "NightShift/cases";
    internal const string RestNightShiftStartRouting = BaseRestString + "NightShift/start";
    internal const string RestNightShiftChatRouting = BaseRestString + "NightShift/chat";
    internal const string RestNightShiftFinishRouting = BaseRestString + "NightShift/finish";
    internal const string RestNightShiftSessionsRouting = BaseRestString + "NightShift/sessions";
}
