import React from 'react';
import { Layout, Typography, Row, Col, Space } from 'antd';
import GetStartedButton from './GetStartedButton';
import SignInButton from './SignInButton';

const { Header, Content } = Layout;
const { Title, Paragraph } = Typography;

const HomePage = () => {
    return (
        <Layout className="layout" style={{ minHeight: '100vh', minWidth: '460px' }}>
            <Header style={{ padding: '0 50px', display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                <div className="logo" style={{ display: 'flex', alignItems: 'center' }}>

                    <Typography.Title level={4} style={{ color: 'white', margin: '0', marginLeft: '8px' }}>
                        Issue Tracker
                    </Typography.Title>
                </div>
                <Space>
                    <SignInButton />
                </Space>
            </Header>

            <Content style={{
                padding: '80px 50px',
                backgroundColor: '#f5f5f5',
                flex: '1 1 auto'
            }}>
                <Row
                    gutter={[{ xs: 16, sm: 24, md: 32, lg: 48 }, { xs: 32, sm: 40, md: 48 }]}
                    align="top"
                    style={{ height: '100%' }}
                >
                    <Col xs={24} sm={24} md={24} lg={12}>
                        <div className="content-section" style={{ marginBottom: '30px' }}>
                            <Title style={{ marginBottom: '24px' }}>Track Issues Efficiently</Title>
                            <Paragraph style={{ fontSize: '16px', marginBottom: '20px' }}>
                                A simple and effective tool to track and manage issues in your projects.
                                Create, assign, and monitor tasks in one convenient place.
                            </Paragraph>
                            <Paragraph style={{ fontSize: '16px', marginBottom: '32px' }}>
                                Keep your team organized and focused on what matters most.
                            </Paragraph>
                            <GetStartedButton />
                        </div>
                    </Col>
                    <Col xs={24} sm={24} md={24} lg={12}>
                        <div className="image-container" style={{ display: 'flex', justifyContent: 'center' }}>
                            <img
                                src="undraw_online-collaboration_xon8.svg"
                                alt="Issue tracker interface"
                                style={{ maxWidth: '100%', height: 'auto' }}
                            />
                        </div>
                    </Col>
                </Row>
            </Content>
        </Layout>
    );
};

export default HomePage;