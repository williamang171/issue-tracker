'use client'

import { LoginOutlined } from '@ant-design/icons'
import { Button } from 'antd'
import { signIn } from 'next-auth/react'
import React from 'react'

export default function SignInButton() {
    return (
        <Button type="primary" icon={<LoginOutlined />} onClick={() => signIn('id-server', { callbackUrl: '/' }, { prompt: 'login' })}>
            Sign In
        </Button>
    )
}
