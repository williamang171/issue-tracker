'use client'

import { Button } from 'antd'
import { signIn } from 'next-auth/react'
import React from 'react'

export default function GetStartedButton() {
    return (
        <Button type='primary' size='large' onClick={() => signIn('id-server', { callbackUrl: '/' }, { prompt: 'login' })}>
            Get Started
        </Button>
    )
}
