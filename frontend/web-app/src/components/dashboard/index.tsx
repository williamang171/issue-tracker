import React from 'react';
import ProjectPieChart from './ProjectPieChart';
import { Col, Row } from 'antd';
import ProjectBarChart from './ProjectBarChart';

export const DashboardCharts = ({
  issuePriorityCountData = [],
  issueStatusCountData = [],
  issueTypeCountData = [],
  issueStatusCountDataIsFetched = false,
  issueTypeCountDataIsFetched = false,
  issuePriorityCountDataIsFetched = false
}: {
  issuePriorityCountData?: any[];
  issueStatusCountData?: any[];
  issueTypeCountData?: any[];
  issueStatusCountDataIsFetched?: boolean
  issueTypeCountDataIsFetched?: boolean,
  issuePriorityCountDataIsFetched?: boolean
}) => {
  return (
    <Row gutter={[24, 24]}>
      <Col sm={24} md={24} lg={24}>
        <ProjectBarChart data={issueStatusCountData} isFetched={issueStatusCountDataIsFetched} title="Issues by Status" />
      </Col>
      <Col xs={24} sm={12} md={12} lg={12} xl={12}>
        <ProjectPieChart data={issueTypeCountData} title="Issues by Type" isFetched={issueTypeCountDataIsFetched} />
      </Col>
      <Col xs={24} sm={12} md={12} lg={12} xl={12}>
        <ProjectPieChart
          data={issuePriorityCountData}
          title="Issues by Priority"
          isFetched={issuePriorityCountDataIsFetched}
        />
      </Col>
    </Row>
  );
};
