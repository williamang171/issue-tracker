"use client"

import dataProviderSimpleRest from "@refinedev/simple-rest";
import { axiosInstance } from "@app/utils/axios-instance";

// const API_URL = "https://api.fake-rest.refine.dev";
const API_URL = "http://localhost:6001/api"

export const dataProvider = dataProviderSimpleRest(API_URL, axiosInstance);
