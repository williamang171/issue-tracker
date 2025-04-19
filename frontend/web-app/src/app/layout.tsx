import type { Metadata } from "next";
import React, { Suspense } from "react";
import { RefineContext } from "./_refine_context";
import { AntdRegistry } from "@ant-design/nextjs-registry";
import { cookies } from "next/headers";

export const metadata: Metadata = {
  title: "Issue Tracker",
  description: "A simple and effective tool to track and manage issues in your projects",
  icons: {
    icon: "/favicon.ico",
  },
};

export default function RootLayout({
  children,
}: Readonly<{
  children: React.ReactNode;
}>) {
  const cookieStore = cookies();
  const theme = cookieStore.get("theme");

  return (
    <html lang="en">
      <body>
        <Suspense>
          <AntdRegistry>
            <RefineContext defaultMode={theme?.value}>
              {children}
            </RefineContext>
          </AntdRegistry>
        </Suspense>
      </body>
    </html>
  );
}
