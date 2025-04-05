'use client';

import { DashboardCharts } from '@components/dashboard';
import IssueList from '@components/issue/IssueList';
import { UsersList } from '@components/project-assignments/List';
import { useGetProjectChartsData } from '@hooks/useGetProjectChartsData';
import { Edit, useForm } from '@refinedev/antd';
import { useMediaQuery } from 'react-responsive';

import { Alert, Form, Input, Row, Col } from 'antd';
import { GoBack } from '@components/goback';
import { useGetUserRole } from '@hooks/useGetUserRole';
import { useSession } from 'next-auth/react';

export default function ProjectEdit() {
  const { formProps, saveButtonProps, query: queryResult } = useForm({});

  const { isAdmin } = useGetUserRole();

  const { issuePriorityCountData, issueStatusCountData, issueTypeCountData } =
    useGetProjectChartsData();
  const isDesktop = useMediaQuery({ minWidth: 768 });

  if (queryResult?.status === 'error') {
    return <Alert type="error" message="Not Found" />;
  }

  return (
    <div>
      <Row gutter={[24, 24]}>
        <Col md={10} lg={10} xl={12} sm={24} xs={24}>
          <Edit
            title={
              <GoBack
                title='Project Details'
                href='/projects'
              />
            }
            breadcrumb={false}
            saveButtonProps={{
              ...saveButtonProps,
              disabled: !isAdmin
            }}
            isLoading={queryResult?.status === 'loading'}
            headerButtons={<div />}
            goBack={null}
            canDelete
          >
            <Form disabled={!isAdmin} {...formProps} layout="vertical">
              <Form.Item
                label={'Name'}
                name={['name']}
                rules={[
                  {
                    required: true,
                  },
                ]}
              >
                <Input />
              </Form.Item>
              <Form.Item
                label={'Description'}
                name="description"
                rules={[
                  {
                    required: true,
                  },
                ]}
              >
                <Input.TextArea rows={5} />
              </Form.Item>
            </Form>
          </Edit>
          <div style={{ marginBottom: '24px' }} />
          {queryResult?.data?.data.id ? (
            <UsersList projectId={queryResult?.data?.data.id} />
          ) : null}
        </Col>
        <Col
          md={14}
          lg={14}
          xl={12}
          sm={24}
          style={{ marginTop: isDesktop ? '52px' : '0px' }}
        >
          <DashboardCharts
            issuePriorityCountData={issuePriorityCountData}
            issueStatusCountData={issueStatusCountData}
            issueTypeCountData={issueTypeCountData}
          />
        </Col>
        <Col sm={24} md={24}>
          {queryResult?.data?.data.id ? (
            <IssueList projectId={queryResult?.data?.data.id} />
          ) : null}
        </Col>
      </Row>

      <div style={{ marginBottom: '24px' }} />
    </div>
  );
}
