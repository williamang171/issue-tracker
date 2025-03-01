'use client'

import { Button } from 'antd'
import { signIn } from 'next-auth/react'
import React from 'react'

export default function LoginButton() {
  return (
    <Button onClick={() => signIn('id-server', { callbackUrl: '/' }, { prompt: 'login' })}>
      Login
    </Button>
  )
}
