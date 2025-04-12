import { AccessControlProvider } from '@refinedev/core';

export const canAccess = { can: true };
export const cannotAccess = { can: false, reason: ' ' };

export const accessControlProvider: AccessControlProvider = {
  can: async ({ action, params, resource }) => {
    let role = sessionStorage.getItem('role');
    if (!role) {
      return cannotAccess;
    }
    if (role === 'Admin') {
      return canAccess;
    }
    if (resource === 'users') {
      return cannotAccess;
    }

    if (role === 'Member') {
      return handleForMember(action, resource);
    }

    if (role === 'Viewer') {
      return handleForViewer(action);
    }

    return cannotAccess;
  },
};

const handleForMember = (action: string, resource: string | undefined) => {
  if (
    typeof resource === 'string' &&
    ['comments', 'issues', 'attachments'].includes(resource)
  ) {
    return canAccess;
  }

  if (resource === 'projects') {
    if (['delete', 'create'].includes(action)) {
      return cannotAccess;
    }
    return canAccess;
  }
  return cannotAccess;
};

const handleForViewer = (action: string) => {
  if (['create', 'delete'].includes(action)) {
    return cannotAccess;
  }
  return canAccess;
};
