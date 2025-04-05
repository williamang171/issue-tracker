export function useGetUserRole() {
    const roleCode = sessionStorage.getItem("role");
    const isAdmin = roleCode === 'Admin';
    const isMember = roleCode === 'Member';
    const isViewer = roleCode === 'Viewer';
    const isReadOnly = !isAdmin && !isMember;
    return {
        roleCode,
        isAdmin,
        isMember,
        isViewer: isViewer,
        isReadOnly: isReadOnly
    }
}