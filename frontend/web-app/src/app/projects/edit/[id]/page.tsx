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
import { AuditFields } from '@components/fields/audit-fields';

export default function ProjectEdit() {
  const {
    formProps,
    saveButtonProps,
    query: queryResult,
    formLoading,
  } = useForm({
    redirect: false,
  });

  const { isAdmin } = useGetUserRole();

  const { issuePriorityCountData, issueStatusCountData, issueTypeCountData } =
    useGetProjectChartsData();
  const isDesktop = useMediaQuery({ minWidth: 768 });

  if (queryResult?.status === 'error') {
    return <Alert type="error" message="Not Found" />;
  }

  const leftColProps = {
    xl: 10,
    lg: 24,
    md: 24,
    sm: 24,
    xs: 24,
  };
  const rightColProps = {
    xl: 14,
    lg: 24,
    md: 24,
    sm: 24,
    xs: 24,
  };
  const leftTopColProps = {
    ...leftColProps,
    lg: 24,
  };
  const rightTopColProps = {
    ...rightColProps,
    lg: 24,
  };

  return (
    <div>
      <Row gutter={[24, 24]}>
        <Col {...leftTopColProps}>
          <Edit
            title={<GoBack title="Project Details" href="/projects" />}
            breadcrumb={false}
            saveButtonProps={{
              ...saveButtonProps,
              disabled: !isAdmin || formLoading,
            }}
            isLoading={formLoading || queryResult?.status === 'loading'}
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
              <AuditFields />
            </Form>
          </Edit>
        </Col>
        <Col {...rightTopColProps}>
          {queryResult?.data?.data.id ? (
            <>
              {isDesktop ? <div style={{ marginBottom: '48px' }} /> : null}
              <IssueList projectId={queryResult?.data?.data.id} />
            </>
          ) : null}
        </Col>
        <Col {...leftColProps}>
          <DashboardCharts
            issuePriorityCountData={issuePriorityCountData}
            issueStatusCountData={issueStatusCountData}
            issueTypeCountData={issueTypeCountData}
          />
        </Col>
        <Col {...rightColProps}>
          {queryResult?.data?.data.id ? (
            <UsersList projectId={queryResult?.data?.data.id} />
          ) : null}
        </Col>
      </Row>

      <div style={{ marginBottom: '24px' }} />
    </div>
  );
}
