export const FormText = (props: { value?: string }) => {
    const { value } = props;
    return <span className="ant-form-text">{value || '-'}</span>
}