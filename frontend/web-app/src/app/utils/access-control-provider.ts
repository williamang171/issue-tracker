import { AccessControlProvider } from '@refinedev/core';
import {
  API_URL,
} from '@providers/data-provider/data-provider.client';

export const canAccess = { can: true };
export const cannotAccess = { can: false, reason: ' ' };

export const fetchRoleAndSaveToSessionStorage = async (accessToken?: string) => {
  if (!accessToken) {
    return;
  }
  await fetch(`${API_URL}/users/getCurrentUserRole`, {
    method: 'GET',
    headers: {
      'Content-Type': 'application/json',
      authorization: `Bearer ${accessToken}`,
    },
  })
    .then(async (data) => {
      const json = await data.json();
      sessionStorage.setItem('role', json.roleCode);
    })
    .catch((err) => {
      console.log(err);
    });
};

export const getRoleFromSessionStorage = async () => {
  const maxRetries = 10;
  const retryDelay = 500;
  let role = sessionStorage.getItem('role');
  let retryCount = 0;

  while (!role && retryCount < maxRetries) {
    await new Promise(resolve => setTimeout(resolve, retryDelay));
    role = sessionStorage.getItem('role');
    retryCount++;
  }

  return role;
};

export const accessControlProvider: AccessControlProvider = {
  can: async ({ action, params, resource }) => {
    let role = await getRoleFromSessionStorage();
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
