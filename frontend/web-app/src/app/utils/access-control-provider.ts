import { AccessControlProvider } from '@refinedev/core';
import {
  API_URL,
} from '@providers/data-provider/data-provider.client';
import axios from 'axios';

export const canAccess = { can: true };
export const cannotAccess = { can: false, reason: ' ' };

export const fetchRoleWithRetry = async (accessToken: string, maxRetries = 5) => {
  let role = null;
  try {
    const res = await axios.get(`${API_URL}/users/getCurrentUserRole`, {
      headers: {
        'Content-Type': 'application/json',
        authorization: `Bearer ${accessToken}`,
      },
    });
    role = res.data.roleCode;
    return role;
  } catch (err) {
    console.log(err);
  }

  let retryDelay = 500;
  let retryCount = 0;
  while (!role && retryCount < maxRetries) {
    try {
      retryCount++;
      await new Promise(resolve => setTimeout(resolve, retryDelay));
      const res = await axios.get(`${API_URL}/users/getCurrentUserRole`, {
        headers: {
          'Content-Type': 'application/json',
          authorization: `Bearer ${accessToken}`,
        },
      });
      role = res.data.roleCode;
    } catch (err) {
      console.log(err);
    }
  }
  return role;
};

export const accessControlProvider: AccessControlProvider = {
  can: async ({ action, params, resource }) => {
    let role = localStorage.getItem("role");
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
