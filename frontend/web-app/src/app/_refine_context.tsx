'use client';

import React, { useEffect, useState } from 'react';
import { useNotificationProvider } from '@refinedev/antd';
import { type AuthBindings, Refine } from '@refinedev/core';
import { RefineKbar, RefineKbarProvider } from '@refinedev/kbar';
import routerProvider from '@refinedev/nextjs-router';
import { SessionProvider, signIn, signOut, useSession } from 'next-auth/react';
import { ColorModeContextProvider } from '@contexts/color-mode';
import {
  API_URL,
  dataProvider,
} from '@providers/data-provider/data-provider.client';
import '@refinedev/antd/dist/reset.css';
import { axiosInstance } from './utils/axios-instance';
import { usePathname, useRouter } from 'next/navigation';
import {
  ProjectOutlined,
  TeamOutlined,
  UnorderedListOutlined,
} from '@ant-design/icons';
import Loading from '@components/loading/Loading';
import { accessControlProvider, fetchRoleAndSaveToSessionStorage } from './utils/access-control-provider';
import HomePage from '@components/home';

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

const App = ({ children, defaultMode }: React.PropsWithChildren<AppProps>) => {
  const { data, status } = useSession();
  const to = usePathname();
  const [fetchedRole, setFetchedRole] = useState(false);

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
    if (!data?.accessToken) {
      return;
    }
    if (fetchedRole) {
      return;
    }
    await fetchRoleAndSaveToSessionStorage(data?.accessToken).then(() => {
      setFetchedRole(true);
    }).catch((err) => {
      console.error(err);
      setFetchedRole(true);
    });
  };

  useEffect(() => {
    fetchRole();
  }, [data?.accessToken]);

  if (status === 'loading') {
    return <Loading />;
  }

  if (status !== 'authenticated' && to === '/') {
    return <HomePage />;
  }

  if (!fetchedRole) {
    return <Loading />;
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
      setFetchedRole(false);

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
        redirectTo: '/',
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
            accessControlProvider={accessControlProvider}
            resources={[
              {
                name: 'projects',
                list: '/projects',
                create: '/projects/create',
                edit: '/projects/edit/:id',
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
