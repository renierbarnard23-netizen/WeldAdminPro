using WeldAdminPro.Core.Quality.Enums;
using WeldAdminPro.Core.Quality.Models;
using WeldAdminPro.Core.Quality.Services;
using WeldAdminPro.Core.Security.Abstractions;

namespace WeldAdminPro.Core.Tests;

public class HoldPointWorkflowTests
{
    [Fact]
    public async Task ApproveAsync_UnauthenticatedUser_ThrowsUnauthorized()
    {
        var service = CreateService(
            isAuthenticated: false,
            hasPermission: true);

        var holdPoint = CreateHoldPoint(HoldPointStatus.Pending);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => service.ApproveAsync(
                holdPoint,
                "Test Approver",
                "Unauthorized approval"));

        Assert.Equal(
            HoldPointStatus.Pending,
            holdPoint.Status);
    }

    [Fact]
    public async Task ApproveAsync_UserWithoutPermission_ThrowsUnauthorized()
    {
        var service = CreateService(
            isAuthenticated: true,
            hasPermission: false);

        var holdPoint = CreateHoldPoint(HoldPointStatus.Pending);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => service.ApproveAsync(
                holdPoint,
                "Test Approver",
                "Unauthorized approval"));

        Assert.Equal(
            HoldPointStatus.Pending,
            holdPoint.Status);
    }

    [Fact]
    public async Task RejectAsync_UserWithoutPermission_ThrowsUnauthorized()
    {
        var service = CreateService(
            isAuthenticated: true,
            hasPermission: false);

        var holdPoint = CreateHoldPoint(HoldPointStatus.Pending);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => service.RejectAsync(
                holdPoint,
                "Unauthorized rejection"));

        Assert.Equal(
            HoldPointStatus.Pending,
            holdPoint.Status);
    }
    [Fact]
    public async Task ApproveAsync_PendingHoldPoint_ApprovesSuccessfully()
    {
        var service = CreateService();
        var holdPoint = CreateHoldPoint(HoldPointStatus.Pending);

        await service.ApproveAsync(
            holdPoint,
            "Test Approver",
            "Approval test");

        Assert.Equal(
            HoldPointStatus.Approved,
            holdPoint.Status);

        Assert.Equal(
            "Test Approver",
            holdPoint.ApprovedBy);

        Assert.NotNull(holdPoint.ApprovedDate);

        Assert.Equal(
            "Approval test",
            holdPoint.Comments);
    }

    [Fact]
    public async Task RejectAsync_PendingHoldPoint_RejectsSuccessfully()
    {
        var service = CreateService();
        var holdPoint = CreateHoldPoint(HoldPointStatus.Pending);

        await service.RejectAsync(
            holdPoint,
            "Rejection test");

        Assert.Equal(
            HoldPointStatus.Rejected,
            holdPoint.Status);

        Assert.Equal(
            string.Empty,
            holdPoint.ApprovedBy);

        Assert.Null(holdPoint.ApprovedDate);

        Assert.Equal(
            "Rejection test",
            holdPoint.Comments);
    }

    [Fact]
    public async Task RejectAsync_ApprovedHoldPoint_Throws()
    {
        var service = CreateService();
        var holdPoint = CreateHoldPoint(
            HoldPointStatus.Approved);

        holdPoint.ApprovedBy = "Existing Approver";
        holdPoint.ApprovedDate = DateTime.UtcNow;

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.RejectAsync(
                holdPoint,
                "Invalid rejection"));

        Assert.Contains(
            "Only Pending Hold Points can be approved or rejected",
            exception.Message);

        Assert.Equal(
            HoldPointStatus.Approved,
            holdPoint.Status);
    }

    [Fact]
    public async Task ApproveAsync_ApprovedHoldPoint_Throws()
    {
        var service = CreateService();
        var holdPoint = CreateHoldPoint(
            HoldPointStatus.Approved);

        holdPoint.ApprovedBy = "Existing Approver";
        holdPoint.ApprovedDate = DateTime.UtcNow;

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.ApproveAsync(
                holdPoint,
                "Another Approver",
                "Invalid approval"));

        Assert.Equal(
            HoldPointStatus.Approved,
            holdPoint.Status);
    }

    [Fact]
    public async Task ApproveAsync_RejectedHoldPoint_Throws()
    {
        var service = CreateService();
        var holdPoint = CreateHoldPoint(
            HoldPointStatus.Rejected);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.ApproveAsync(
                holdPoint,
                "Test Approver",
                "Invalid re-approval"));

        Assert.Equal(
            HoldPointStatus.Rejected,
            holdPoint.Status);
    }

    [Fact]
    public async Task RejectAsync_RejectedHoldPoint_Throws()
    {
        var service = CreateService();
        var holdPoint = CreateHoldPoint(
            HoldPointStatus.Rejected);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.RejectAsync(
                holdPoint,
                "Invalid second rejection"));

        Assert.Equal(
            HoldPointStatus.Rejected,
            holdPoint.Status);
    }

    private static HoldPointWorkflowService CreateService(
        bool isAuthenticated = true,
        bool hasPermission = true)
    {
        return new HoldPointWorkflowService(
            new TestCurrentUserContext(isAuthenticated),
            new TestPermissionAuthorizationService(hasPermission));
    }

    private static WeldHoldPoint CreateHoldPoint(
        HoldPointStatus status)
    {
        return new WeldHoldPoint
        {
            Id = Guid.NewGuid(),
            WeldId = Guid.NewGuid(),
            HoldPointType = HoldPointType.FitUpInspection,
            Category = HoldPointCategory.Hold,
            RequiredApproverRole = HoldPointApproverRole.QA,
            Status = status,
            IsMandatory = true,
            ApprovedBy = string.Empty,
            ApprovedDate = null,
            Comments = string.Empty
        };
    }

    private sealed class TestCurrentUserContext : ICurrentUserContext
    {
        private readonly bool _isAuthenticated;

        public TestCurrentUserContext(bool isAuthenticated)
        {
            _isAuthenticated = isAuthenticated;
        }

        public bool IsAuthenticated => _isAuthenticated;

        public string UserId => "test-user";

        public string Username => "test";

        public string FullName => "Test User";

        public string Role => "QA Inspector";

        public Guid? CompanyId => Guid.NewGuid();
    }

    private sealed class TestPermissionAuthorizationService
        : IPermissionAuthorizationService
    {
        private readonly bool _hasPermission;

        public TestPermissionAuthorizationService(bool hasPermission)
        {
            _hasPermission = hasPermission;
        }

        public Task<bool> HasPermissionAsync(
            string userId,
            string role,
            string permissionKey)
        {
            return Task.FromResult(_hasPermission);
        }
    }
}

