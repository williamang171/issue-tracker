import dataProviderSimpleRest, { axiosInstance } from "@refinedev/simple-rest";
// import { axiosInstance } from "@app/utils/axios-instance";
import { DataProvider, MetaQuery } from "@refinedev/core";
import { getSession } from "next-auth/react";
import { auth } from "@app/api/auth/[...nextauth]/options";

// const API_URL = "https://api.fake-rest.refine.dev";
const API_URL = "http://localhost:6001/api"

const dataProvider = dataProviderSimpleRest(API_URL, axiosInstance);

async function getHeaders() {
    const session = await auth();
    // const session = await getSession();
    const headers = {
        'Content-type': 'application/json'
    } as any;
    if (session?.accessToken) {
        headers.Authorization = 'Bearer ' + session.accessToken
    }
    return headers;
};

async function getMetaWithHeaders(meta: MetaQuery | undefined) {
    const headers = await getHeaders();
    return {
        ...meta,
        headers,
    }
};

const dataProviderServer = (): Omit<
    Required<DataProvider>,
    "createMany" | "updateMany" | "deleteMany"
> => ({
    ...dataProvider,
    getList: async ({ resource, pagination, filters, sorters, meta }) => {
        const metaWithHeaders = await getMetaWithHeaders(meta);
        return dataProvider.getList({ resource, pagination, filters, sorters, meta: metaWithHeaders });
    },

    getMany: async ({ resource, ids, meta }) => {
        const metaWithHeaders = await getMetaWithHeaders(meta);
        return dataProvider.getMany({ resource, ids, meta: metaWithHeaders });
    },

    create: async ({ resource, variables, meta }) => {
        const metaWithHeaders = await getMetaWithHeaders(meta);
        return dataProvider.create({ resource, variables, meta: metaWithHeaders });
    },

    update: async ({ resource, id, variables, meta }) => {
        const metaWithHeaders = await getMetaWithHeaders(meta);
        return dataProvider.update({ resource, id, variables, meta: metaWithHeaders });
    },

    getOne: async ({ resource, id, meta }) => {
        const metaWithHeaders = await getMetaWithHeaders(meta);
        return dataProvider.getOne({ resource, id, meta: metaWithHeaders });
    },

    deleteOne: async ({ resource, id, variables, meta }) => {
        const metaWithHeaders = await getMetaWithHeaders(meta);
        return dataProvider.deleteOne({
            resource, id, variables, meta: metaWithHeaders
        })
    },
});

export const dataProviderServerInstance = dataProviderServer();