using System.Text.Json;
using RESR.Models.Comments;

namespace RESR.WebAPI.Tests.Comments;

public sealed class CreateCommentRequestSerializationTests
{
    [Fact]
    public void Deserialize_AllowsMissingParentComment()
    {
        const string json = """
        {
          "content": "Hello"
        }
        """;

        var request = JsonSerializer.Deserialize<CreateCommentRequest>(
            json,
            new JsonSerializerOptions(JsonSerializerDefaults.Web)
        );

        Assert.NotNull(request);
        Assert.Equal("Hello", request!.Content);
        Assert.Null(request.IdParentComment);
    }
}
