using RESR.Core.Controllers.Comments.Factories;

namespace RESR.Core.Tests.Comments.Factories;

public sealed class CommentFactoryTests
{
    [Fact]
    public void CreateForCreation_AssignsExpectedFields()
    {
        var factory = new CommentFactory();

        var comment = factory.CreateForCreation("Hello", 4, 2, 1);

        Assert.Equal("Hello", comment.Content);
        Assert.Equal(4, comment.IdResource);
        Assert.Equal(2, comment.IdUser);
        Assert.Equal(1, comment.IdParentComment);
    }

    [Fact]
    public void CreateFromPersistence_AssignsExpectedFields()
    {
        var factory = new CommentFactory();
        var createdAt = new DateTime(2026, 3, 11, 12, 0, 0, DateTimeKind.Utc);
        var modifiedAt = createdAt.AddMinutes(10);

        var comment = factory.CreateFromPersistence(9, "Hello", createdAt, modifiedAt, null, 4, 2, null);

        Assert.Equal(9, comment.IdComment);
        Assert.Equal(createdAt, comment.CreatedAt);
        Assert.Equal(modifiedAt, comment.ModifiedAt);
        Assert.Equal(4, comment.IdResource);
        Assert.Equal(2, comment.IdUser);
        Assert.Null(comment.IdParentComment);
    }
}
