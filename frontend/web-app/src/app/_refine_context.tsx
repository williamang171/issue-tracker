'use client';

import React, { useEffect, useMemo, useState } from 'react';
import { useNotificationProvider } from '@refinedev/antd';
import { type AuthBindings, Refine } from '@refinedev/core';
import { RefineKbar, RefineKbarProvider } from '@refinedev/kbar';
import routerProvider from '@refinedev/nextjs-router';
import { SessionProvider, signIn, signOut, useSession } from 'next-auth/react';
import { ColorModeContextProvider } from '@contexts/color-mode';
import { dataProvider } from '@providers/data-provider/data-provider.client';
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
import { Spin } from 'antd';
import Loading from '@components/loading/Loading';

type RefineContextProps = {
  defaultMode?: string;
};
const contentStyle: React.CSSProperties = {
  padding: 50,
  background: 'rgba(0, 0, 0, 0.05)',
  borderRadius: 4,
};

const content = <div style={contentStyle} />;

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

const App = async ({
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
                name: 'IssuesRoot',
                meta: {
                  label: 'Issues',
                },
                icon: <ReconciliationOutlined />,
              },
              {
                name: 'issues',
                list: '/issues',
                create: '/issues/create',
                edit: '/issues/edit/:id',
                identifier: 'issues',
                meta: {
                  canDelete: true,
                  label: 'All Issues',
                  parent: 'IssuesRoot',
                  icon: <UnorderedListOutlined />,
                },
              },
              {
                name: 'issues',
                list: '/issues/self',
                create: '/issues/create',
                edit: '/issues/edit/:id',
                identifier: 'issues-self',
                meta: {
                  canDelete: true,
                  label: 'My Issues',
                  parent: 'IssuesRoot',
                  icon: <AuditOutlined />,
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
