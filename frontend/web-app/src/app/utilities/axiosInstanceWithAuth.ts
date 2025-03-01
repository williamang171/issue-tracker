import axios from 'axios';
import { auth } from "@app/api/auth/[...nextauth]/options";

export const axiosInstanceWithAuth = axios.create();

// add token to every request
// axiosInstanceWithAuth.interceptors.request.use(
//     async (config) => {
//         const session = await auth();
//         const token = session?.accessToken;
//         if (token && config?.headers) {
//             config.headers.Authorization = `Bearer ${token}`;
//         }
//         return config;
//     },
//     (error) => {
//         return Promise.reject(error);
//     }
// );
