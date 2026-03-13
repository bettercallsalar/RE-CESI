using RESR.MAUI.Services;

namespace RESR.MAUI.Tests.Services;

public sealed class ApiSessionTests
{
    [Fact]
    public void Token_WhenSet_PersistsAcrossSessions()
    {
        PreferencesShim.Remove("auth_token");

        var session = new ApiSession { Token = "jwt-token" };

        var nextSession = new ApiSession();

        Assert.Equal("jwt-token", nextSession.Token);
    }

    [Fact]
    public void Token_WhenCleared_RemovesStoredValue()
    {
        PreferencesShim.Remove("auth_token");

        var session = new ApiSession { Token = "jwt-token" };

        session.Token = " ";

        var nextSession = new ApiSession();

        Assert.Null(nextSession.Token);
    }
}
