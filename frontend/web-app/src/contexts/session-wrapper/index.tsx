"use client";

import { axiosInstance } from "@app/utils/axios-instance";
import React, {
  type PropsWithChildren,
  createContext,
  useEffect,
  useState,
} from "react";
import { useSession, signOut } from "next-auth/react";
import { fetchRoleWithRetry } from "@app/utils/access-control-provider";
import Loading from "@components/loading/Loading";
import { usePathname, useRouter } from "next/navigation";
import HomePage from "@components/home";

type SessionWrapperContextType = {
  role: string
};

export const SessionWrapperContext = createContext<SessionWrapperContextType>(
  {} as SessionWrapperContextType,
);

type SessionWrapperContextProvider = {
  defaultMode?: string;
};

export const SessionWrapperContextProvider: React.FC<
  PropsWithChildren<SessionWrapperContextProvider>
> = ({ children }) => {
  const { data, status } = useSession();
  const to = usePathname();
  const [role, setRole] = useState("");
  const [fetchingRole, setFetchingRole] = useState(false);
  const { push } = useRouter();

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
    if (role) {
      return;
    }
    if (!data?.accessToken) {
      return;
    }
    if (fetchingRole) {
      return;
    }
    setFetchingRole(true);
    await fetchRoleWithRetry(data?.accessToken)
      .then((roleCode: string | null | undefined) => {
        setFetchingRole(false);
        if (roleCode) {
          setRole(roleCode);
          localStorage.setItem('role', roleCode);
        }
        else {
          signOut({
            redirect: true,
            redirectTo: '/',
          });
        }
      }).catch((err) => {
        setFetchingRole(false);
        signOut({
          redirect: true,
          redirectTo: '/',
        });
        console.error(err);
      });
  };
  useEffect(() => {
    if (data?.accessToken) {
      fetchRole();
    }
  }, [data?.accessToken]);

  useEffect(() => {
    if (status === 'unauthenticated' && to !== '/') {
      push('/');
      return;
    }
  }, [status, to])

  if (status === 'loading') {
    return <Loading />;
  }

  if (status === 'authenticated' && !role) {
    return <Loading />;
  }

  if (status === 'unauthenticated' && to === '/') {
    return <HomePage />;
  }

  return (
    <SessionWrapperContext.Provider
      value={{
        role
      }}
    >
      {children}
    </SessionWrapperContext.Provider>
  );
};
