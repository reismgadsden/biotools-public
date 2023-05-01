param(
    [string] $seqType,
    [string[]] $Left,
    [string[]] $Right,
    [string] $Output,
    [string] $User,
    [string] $Email
)
sudo python3 AzureEmail.py "${Email}" "${User}" "Trinity Job Notification" "A Trinity job has been started."
sudo rm -rf trinity_out*
sudo rm -rf TrinityResources\*

try {

    if ($Output -ne "") {
        $Output = "${Output}/"
    }

    $ctx = New-AzStorageContext -ConnectionString ""

    foreach ($l in $left) {
        $LeftBlobs = @{
            Blob = "${l}"
            Container = $User
            Destination = '.\TrinityResources\'
            Context= $ctx
        }
        Get-AzStorageBlobContent @LeftBlobs
    }

    foreach ($r in $right) {
        $RightBlobs = @{
            Blob = "${r}"
            Container = $User
            Destination = '.\TrinityResources\'
            Context= $ctx
        }
        Get-AzStorageBlobContent @RightBlobs
    }

    $left_reads = ($left | ForEach-Object {"`"$pwd/TrinityResources/$_`""}) -join ", "
    $right_reads = ($right | ForEach-Object {"`"$pwd/TrinityResources/$_`""}) -join ", "

    $cmd =  "sudo docker run --rm -v${pwd}:${pwd} trinityrnaseq/trinityrnaseq Trinity --seqType fq --left ${left_reads} --right ${right_reads} --max_memory 4G --CPU 2 --output ${pwd}/trinity_out_dir"
    Invoke-Expression -command $cmd
    $Blob1HT = @{
        File = './trinity_out_dir.Trinity.fasta'
        Container = $User
        Blob = "${Output}Trinity.fasta"
        Context = $ctx
        StandardBlobTier = 'Hot'
    }
    Set-AzStorageBlobContent @Blob1HT

    sudo python3 AzureEmail.py "${Email}" "${User}" "Trinity Job Complete" "Your Trinity job has finished without fail."

    sudo rm -rf trinity_out*
    sudo rm -rf TrinityResources\*
}
catch {
   sudo python3 AzureEmail.py "${Email}" "${User}" "Trinity Job Failed" "Your Trinity job failed with the following exception: `n${_}"
   sudo rm -rf trinity_out*
   sudo rm -rf TrinityResources\*
}
