"use client";

import { axiosInstance } from "@app/utils/axios-instance";
import React, {
  type PropsWithChildren,
  createContext,
  useEffect,
  useState,
} from "react";
import { useSession } from "next-auth/react";
import { fetchRoleAndSaveToCache } from "@app/utils/access-control-provider";
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
  const [showHome, setShowHome] = useState(false);
  const [mounted, setIsMounted] = useState(false);
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
    if (!mounted) {
      return;
    }
    await fetchRoleAndSaveToCache(data?.accessToken)
      .then((roleCode: string | null | undefined) => {
        if (roleCode) {
          setRole(roleCode);
          localStorage.setItem('role', roleCode);
        }
        else {
          setShowHome(true);
        }
      }).catch((err) => {
        setShowHome(true);
        console.error(err);
      });
  };

  useEffect(() => {
    setIsMounted(true);
  }, [])

  useEffect(() => {
    fetchRole();
  }, [data?.accessToken, mounted]);

  useEffect(() => {
    if (status === 'unauthenticated') {
      push('/');
    }
  }, [status])

  if (status === 'loading') {
    return <Loading />;
  }

  if (showHome) {
    return <HomePage />;
  }

  if (status === 'authenticated' && !role) {
    return <Loading />;
  }

  if (status !== 'authenticated' && to === '/') {
    return <HomePage />;
  }

  if (status === 'unauthenticated') {
    return <Loading />
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
