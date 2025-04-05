import NextAuth, { Profile } from "next-auth"
import { OIDCConfig } from 'next-auth/providers'
import DuendeIDS6Provider from "next-auth/providers/duende-identity-server6"
import axios from 'axios';
import { API_URL } from "@providers/data-provider/data-provider.client";

const fetchRole = async (accessToken: string) => {
  await fetch(`${API_URL}/users/getCurrentUserRole`, {
    method: 'GET',
    headers: {
      'Content-Type': 'application/json',
      authorization: `Bearer ${accessToken}`
    },
  }).then(async (data) => {
    const json = await data.json();
    return json.roleCode;
  });
}

export const { handlers, signIn, signOut, auth } = NextAuth({
  session: {
    strategy: 'jwt'
  },
  events: {
    signIn: ({ account, profile }) => {
      axios.post('http://localhost:6001/api/users/sync', {}, {
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
        url: `${process.env.ID_URL}/connect/token`
      },
      userinfo: {
        url: `${process.env.ID_URL}/connect/token`
      },

      idToken: true
    } as OIDCConfig<Omit<Profile, 'username'>>),
  ],
  callbacks: {
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
      // token.roleCode = await fetchRole(token.accessToken);
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