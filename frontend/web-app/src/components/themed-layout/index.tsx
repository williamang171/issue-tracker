"use client";

import { Header } from "@components/header";
import { Title } from "@components/title";
import { ThemedLayoutV2 } from "@refinedev/antd";
import React from "react";

export const ThemedLayout = ({ children }: React.PropsWithChildren) => {
  return (
    <ThemedLayoutV2 Header={() => <Header sticky />} Title={Title}>{children}</ThemedLayoutV2>
  );
};
