'use client';

import React, { act, useEffect } from 'react';
import { useNotificationProvider } from '@refinedev/antd';
import { type AuthBindings, Refine } from '@refinedev/core';
import { RefineKbar, RefineKbarProvider } from '@refinedev/kbar';
import routerProvider from '@refinedev/nextjs-router';
import { SessionProvider, signIn, signOut, useSession } from 'next-auth/react';
import { ColorModeContextProvider } from '@contexts/color-mode';
import { API_URL, dataProvider } from '@providers/data-provider/data-provider.client';
import '@refinedev/antd/dist/reset.css';
import { axiosInstance } from './utils/axios-instance';
import { usePathname } from 'next/navigation';
import {
  AuditOutlined,
  ProjectOutlined,
  ReconciliationOutlined,
  TeamOutlined,
  UnorderedListOutlined,
} from '@ant-design/icons';
import HomePage from '@components/home';
import Loading from '@components/loading/Loading';

const canAccess = { can: true };
const cannotAccess = { can: false, reason: ' ' };

type RefineContextProps = {
  defaultMode?: string;
};

export const RefineContext = (
  props: React.PropsWithChildren<RefineContextProps>
) => {
  return (
    <SessionProvider refetchOnWindowFocus={false}>
      <App {...props} />
    </SessionProvider>
  );
};

type AppProps = {
  defaultMode?: string;
};

const App = ({
  children,
  defaultMode,
}: React.PropsWithChildren<AppProps>) => {
  const { data, status } = useSession();
  const to = usePathname();

  axiosInstance.interceptors.request.clear();
  axiosInstance.interceptors.request.use(
    async (config) => {
      const token = data?.accessToken;
      if (token && config?.headers) {
        config.headers.Authorization = `Bearer ${token}`;
      }
      return config;
    },
    (error) => {
      return Promise.reject(error);
    }
  );

  const fetchRole = async () => {
    if (data?.accessToken) {
      await fetch(`${API_URL}/users/getCurrentUserRole`, {
        method: 'GET',
        headers: {
          'Content-Type': 'application/json',
          authorization: `Bearer ${data?.accessToken}`
        },
      }).then(async (data) => {
        const json = await data.json();
        sessionStorage.setItem('role', json.roleCode)
      });
    }
  }

  if (status === 'loading') {
    return <Loading />;
  }

  if (status === 'unauthenticated') {
    return <HomePage />;
  }

  const authProvider: AuthBindings = {
    login: async ({ providerName, email, password }: any) => {
      if (providerName) {
        signIn(providerName, {
          callbackUrl: to ? to.toString() : '/',
          redirect: true,
        });

        return {
          success: true,
        };
      }

      const signUpResponse = await signIn('CredentialsSignUp', {
        email,
        password,
        callbackUrl: to ? to.toString() : '/',
        redirect: false,
      });

      if (!signUpResponse) {
        return {
          success: false,
        };
      }

      const { ok, error } = signUpResponse;

      if (ok) {
        return {
          success: true,
          redirectTo: '/',
        };
      }

      return {
        success: false,
        error: new Error(error?.toString()),
      };
    },
    logout: async () => {
      sessionStorage.removeItem('role');
      signOut({
        redirect: true,
        callbackUrl: '/',
      });

      return {
        success: true,
      };
    },
    onError: async (error: any) => {
      if (error.response?.status === 401) {
        return {
          logout: true,
        };
      }

      return {
        error,
      };
    },
    check: async () => {
      return {
        authenticated: status === 'authenticated',
      };
    },
    getPermissions: async () => {
      return null;
    },
    getIdentity: async () => {
      if (data?.user) {
        const { user } = data;
        return {
          username: user.username,
          name: user.name,
          avatar: user.image,
        };
      }

      return null;
    },
  };

  return (
    <>
      <RefineKbarProvider>
        <ColorModeContextProvider defaultMode={defaultMode}>
          <Refine
            routerProvider={routerProvider}
            dataProvider={dataProvider}
            notificationProvider={useNotificationProvider}
            authProvider={authProvider}
            accessControlProvider={{
              can: async ({ action, params, resource }) => {
                let role = sessionStorage.getItem("role");
                if (!role) {
                  await fetchRole();
                  role = sessionStorage.getItem("role");
                }
                if (role === "Admin") {
                  return canAccess;
                }
                if (resource === "users") {
                  return cannotAccess;
                }

                if (role === "Member") {
                  if (resource === "comments") {
                    return canAccess;
                  }

                  if (resource === "issues") {
                    return canAccess;
                  }

                  if (resource === "attachments") {
                    return canAccess;
                  }

                  if (resource === "projects") {
                    if (["delete", "create"].includes(action)) {
                      return cannotAccess;
                    }

                    return canAccess;
                  }
                }

                if (role === "Viewer") {
                  if (["create", "delete"].includes(action)) {
                    return cannotAccess;
                  }
                  return canAccess;
                }

                return cannotAccess;
              },
            }}
            resources={[
              {
                name: 'projects',
                list: '/projects',
                create: '/projects/create',
                edit: '/projects/edit/:id',
                show: '/projects/show/:id',

                meta: {
                  icon: <ProjectOutlined />,
                },
              },
              {
                name: 'issues',
                list: '/issues',
                create: '/issues/create',
                edit: '/issues/edit/:id',
                identifier: 'issues',
                meta: {
                  canDelete: true,
                  icon: <UnorderedListOutlined />,
                },
              },
              {
                name: 'users',
                list: '/users',
                create: '/users/create',
                edit: '/users/edit/:id',
                show: '/users/show/:id',
                meta: {
                  icon: <TeamOutlined />,
                  canDelete: true,
                },
              },
            ]}
            options={{
              syncWithLocation: true,
              warnWhenUnsavedChanges: true,
              useNewQueryKeys: true,
            }}
          >
            {children}
            <RefineKbar />
          </Refine>
        </ColorModeContextProvider>

      </RefineKbarProvider>
    </>
  );
};
