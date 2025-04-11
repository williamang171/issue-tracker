import { CreatedBy } from "../created-by"
import { CreatedTime } from "../created-time"
import { LastUpdatedBy } from "../last-updated-by"
import { LastUpdatedTime } from "../last-updated-time"

export const AuditFields = () => {
    return (
        <>
            <CreatedBy />
            <CreatedTime />
            <LastUpdatedBy />
            <LastUpdatedTime />
        </>
    )
}