'use client';

import IssueList from '@components/issue/IssueList';
import { useSession } from 'next-auth/react';

export default function Page() {
    const { data, status } = useSession();
    return <IssueList assignee={data?.user?.name} />
}
