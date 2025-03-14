import React from 'react';
import ProjectPieChart from './ProjectPieChart';
import { Col, Row } from 'antd';
import ProjectBarChart from './ProjectBarChart';

export const DashboardCharts = ({
    issuePriorityCountData = [],
    issueStatusCountData = [],
    issueTypeCountData = [],
}: {
    issuePriorityCountData?: any[];
    issueStatusCountData?: any[];
    issueTypeCountData?: any[];
}) => {
    return (
        <Row gutter={{ xs: 8, sm: 16, md: 24, lg: 32 }}>
            <Col span={8}>
                <ProjectBarChart data={issueStatusCountData} title="Issues by Status" />
            </Col>
            <Col span={8}>
                <ProjectPieChart data={issueTypeCountData} title="Issues by Type" />
            </Col>
            <Col span={8}>
                <ProjectPieChart
                    data={issuePriorityCountData}
                    title="Issues by Priority"
                />
            </Col>
        </Row>
    );
};
