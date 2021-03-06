﻿using Azure.ResourceManager.Core;

namespace azure_proto_authorization
{
    /// <summary>
    /// Extensions for RoleAssignment Containers and Operations
    /// </summary>
    public static class RoleAssignmentExtensions
    {
        /// <summary>
        /// Get RoleAssignment Container for the given resource.  Note that this is only valid for unconstrained role assignments, so
        /// it is a generation-time decision if we include this.
        /// </summary>
        /// <param name="resource">The resource that is the target of the roel assignemnt</param>
        /// <returns>A <see cref="RoleAssignmentContainer"/> that allows creating and listing RoleAssignments</returns>
        public static RoleAssignmentContainer RoleAssignments(this ResourceOperationsBase resource)
        {
            return new RoleAssignmentContainer(resource);
        }

        /// <summary>
        /// Get RoleAssignment Container for the given resource.  Note that this is only valid for unconstrained role assignments, so
        /// it is a generation-time decision if we include this.
        /// </summary>
        /// <param name="resource">The subscription that is the target of the role assignemnt</param>
        /// <returns>A <see cref="RoleAssignmentContainer"/> that allows creating and listing RoleAssignments</returns>
        public static RoleAssignmentContainer RoleAssignments(this SubscriptionOperations resource)
        {
            return new RoleAssignmentContainer(resource);
        }

        /// <summary>
        /// Get RoleAssignment Container for the given resource and scope.  Note that this is only valid for unconstrained role assignments, so
        /// it is a generation-time decision if we include this.
        /// </summary>
        /// <param name="subscription">The subscription containign the role assignment</param>
        /// <param name="scope">The target of the role assignment</param>
        /// <returns>A <see cref="RoleAssignmentContainer"/> that allows creating and listing RoleAssignments</returns>
        public static RoleAssignmentContainer RoleAssigmentsAtScope(this SubscriptionOperations subscription, ResourceIdentifier scope)
        {
            return new RoleAssignmentContainer(subscription.ClientOptions, scope);
        }

        /// <summary>
        /// Get RoleAssignment Container for the given resource and scope.  Note that this is only valid for unconstrained role assignments, so
        /// it is a generation-time decision if we include this.
        /// </summary>
        /// <param name="subscription">The subscription containign the role assignment</param>
        /// <param name="scope">The target of the role assignment</param>
        /// <returns>A <see cref="RoleAssignmentContainer"/> that allows creating and listing RoleAssignments</returns>
        public static RoleAssignmentContainer RoleAssigmentsAtScope(this SubscriptionOperations subscription, Resource scope)
        {
            return new RoleAssignmentContainer(subscription.ClientOptions, scope.Id);
        }

        /// <summary>
        /// Get RoleAssignment Operations for the given resource and scope.  Note that this is only valid for unconstrained role assignments, so
        /// it is a generation-time decision if we include this.
        /// </summary>
        /// <param name="resource">The resource containing the role assignment</param>
        /// <param name="name">The name of the role assignment</param>
        /// <returns>A <see cref="RoleAssignmentOperations"/> that allows getting and deleting RoleAssignments</returns>
        public static RoleAssignmentOperations RoleAssignment(this ResourceOperationsBase resource, string name)
        {
            return new RoleAssignmentOperations(resource.ClientOptions, $"{resource.Id}/providers/Microsoft.Authorization/roleAssignments/{name}");
        }

        /// <summary>
        /// Get RoleAssignment Operations for the given resource and scope.  Note that this is only valid for unconstrained role assignments, so
        /// it is a generation-time decision if we include this.
        /// </summary>
        /// <param name="resource">The subscription containing the role assignment</param>
        /// <param name="name">The name of the role assignment</param>
        /// <returns>A <see cref="RoleAssignmentOperations"/> that allows getting and deleting RoleAssignments</returns>
        public static RoleAssignmentOperations RoleAssignment(this SubscriptionOperations resource, string name)
        {
            return new RoleAssignmentOperations(resource.ClientOptions, $"{resource.Id}/providers/Microsoft.Authorization/roleAssignments/{name}");
        }

        /// <summary>
        /// Get RoleAssignment Operations for the given resource and scope.  Note that this is only valid for unconstrained role assignments, so
        /// it is a generation-time decision if we include this.
        /// </summary>
        /// <param name="resource">The subscription containing the role assignment</param>
        /// <param name="resourceId">The id of the role assignment</param>
        /// <returns>A <see cref="RoleAssignmentOperations"/> that allows getting and deleting RoleAssignments</returns>
        public static RoleAssignmentOperations RoleAssignmentAtScope(this SubscriptionOperations resource, ResourceIdentifier resourceId)
        {
            return new RoleAssignmentOperations(resource.ClientOptions, resourceId);
        }

        /// <summary>
        /// Get RoleAssignment Operations for the given resource and scope.  Note that this is only valid for unconstrained role assignments, so
        /// it is a generation-time decision if we include this.
        /// </summary>
        /// <param name="resource">The subscription containing the role assignment</param>
        /// <param name="role">The object representing the role assignment</param>
        /// <returns>A <see cref="RoleAssignmentOperations"/> that allows getting and deleting RoleAssignments</returns>
        public static RoleAssignmentOperations RoleAssignmentAtScope(this SubscriptionOperations resource, RoleAssignmentData role)
        {
            return new RoleAssignmentOperations(resource.ClientOptions, role);
        }
    }
}
