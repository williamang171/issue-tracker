'use client';

import dataProviderSimpleRest from '@refinedev/simple-rest';
import { axiosInstance } from '@app/utils/axios-instance';

export const API_URL = 'http://localhost:6001/api';

export const dataProvider = dataProviderSimpleRest(API_URL, axiosInstance);
