WeldAdmin Pro W18.1 - Legacy NDT Traceability Review

Purpose
-------
Adds a controlled review/conversion workflow for legacy NDT records that have no equipment traceability.

Changes
-------
1. Legacy NDT records are identified in NDT History as "Legacy / not recorded".
2. An authorised Quality NDT user can open "Review" for a legacy NDT record.
3. The reviewer must confirm that the displayed inspection date is the original examination date.
4. The reviewer selects the actual controlled equipment used.
5. The server validates project/company ownership, active status, and calibration validity independently for every selected item.
6. Equipment/calibration snapshots are attached to the existing NDT record without creating a replacement NDT result.
7. A Weld History event and global Audit Log entry record the review, reviewer, equipment and remarks.
8. Existing release governance remains intact. No release bypass or automatic acceptance was added.

Apply
-----
Stop WeldAdmin Pro, extract these files over the existing repository, then build:

dotnet build .\\WeldAdminPro.sln

Do not run the application until the build is confirmed clean.
