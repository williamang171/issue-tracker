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
import { accessControlProvider, fetchRoleAndSaveToCache } from './utils/access-control-provider';
import HomePage from '@components/home';
import { SessionWrapperContextProvider } from '@contexts/session-wrapper';

type RefineContextProps = {
  defaultMode?: string;
};

export const RefineContext = (
  props: React.PropsWithChildren<RefineContextProps>
) => {
  return (
    <SessionProvider refetchOnWindowFocus={false}>
      <SessionWrapperContextProvider>
        <App {...props} />
      </SessionWrapperContextProvider>
    </SessionProvider>
  );
};

type AppProps = {
  defaultMode?: string;
};

const App = ({ children, defaultMode }: React.PropsWithChildren<AppProps>) => {
  const { data, status } = useSession();
  const to = usePathname();

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
      localStorage.removeItem('role');
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
