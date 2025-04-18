import NextAuth, { Profile } from "next-auth"
import { OIDCConfig } from 'next-auth/providers'
import DuendeIDS6Provider from "next-auth/providers/duende-identity-server6"
import axios from 'axios';

export const API_URL_INTERNAL = process.env.API_URL_INTERNAL || 'http://localhost:6001/api';

export const { handlers, signIn, signOut, auth } = NextAuth({
  session: {
    strategy: 'jwt'
  },
  events: {
    signIn: ({ account, profile }) => {
      axios.post(`${API_URL_INTERNAL}/users/sync`, {}, {
        headers: {
          Authorization: `Bearer ${account?.access_token}`
        }
      }).then(() => {
        console.log(`sync executed successfully for ${profile?.username}`);
      }).catch((err) => {
        console.log(err);
      })
    }
  },
  providers: [
    DuendeIDS6Provider({
      id: 'id-server',
      clientId: "nextApp",
      clientSecret: "secret",
      issuer: process.env.ID_URL,
      authorization: {
        params: { scope: 'openid profile issueTrackerApp' },
        url: process.env.ID_URL + '/connect/authorize'
      },
      token: {
        url: `${process.env.ID_URL_INTERNAL}/connect/token`
      },
      userinfo: {
        url: `${process.env.ID_URL_INTERNAL}/connect/token`
      },
      idToken: true
    } as OIDCConfig<Omit<Profile, 'username'>>),
  ],
  callbacks: {
    async redirect({ url, baseUrl }) {
      return url.startsWith(baseUrl) ? url : baseUrl
    },
    async authorized({ auth }) {
      return !!auth
    },
    async jwt({ token, profile, account, user }) {
      if (account && account.access_token) {
        token.accessToken = account.access_token
      }
      if (profile) {
        token.username = profile.username
      }
      return token;
    },
    async session({ session, token, }) {
      if (token) {
        session.user.username = token.username;
        session.accessToken = token.accessToken;
      }
      return session;
    },
  }
})