import { BaseKey, } from '@refinedev/core';
import React, { useEffect } from 'react';
import { useParams } from 'next/navigation';

import { RESOURCE } from '@app/constants/resource';
import { useModalForm, } from '@refinedev/antd';
import PicturesList from './picturesList';

const resource = RESOURCE.attachments;

export const AttachmentList = () => {
  const params = useParams();
  const issueId = params.id;
  // Create Modal
  const {
    modalProps: createModalProps,
    formProps: createFormProps,
    formLoading: createFormLoading,
    form
  } = useModalForm({
    action: "create",
    resource: resource,
  });
  const setFieldValue = form?.setFieldValue;
  const isOpen = createModalProps?.open;

  useEffect(() => {
    if (setFieldValue && issueId && isOpen) {
      setFieldValue('issueId', issueId);
    }
  }, [isOpen, setFieldValue, issueId]);

  return (
    <>
      <PicturesList />


    </>
  );
};
