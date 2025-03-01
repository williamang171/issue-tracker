'use server'

import { auth } from '../api/auth/[...nextauth]/options'

export async function getCurrentUser() {
    try {
        const session = await auth();

        if (!session) return null;

        return session.user;
    } catch (error) {
        return null;
    }
}