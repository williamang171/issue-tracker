'use client'

import { UserOutlined } from '@ant-design/icons'
import { Button } from 'antd'
import { signIn } from 'next-auth/react'
import React from 'react'

export default function SignUpButton() {
    return (
        <Button type="default" icon={<UserOutlined />} onClick={() => signIn('id-server', { callbackUrl: '/' }, { prompt: 'login' })}>
            Sign Up
        </Button>
    )
}
