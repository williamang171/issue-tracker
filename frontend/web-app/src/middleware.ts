export { auth as middleware } from "./app/api/auth/[...nextauth]/options"

export const config = {
    matcher: [
        '/session'
    ],
    pages: {
        signIn: '/api/auth/signin'
    }
}