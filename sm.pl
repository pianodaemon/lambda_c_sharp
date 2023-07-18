use strict;

sub _wrap_execution {
    my $in_stream = shift;
    my %kvargs = @_;
    my $buff_str;
    {
        my $pid = open my $out_stream, "|-", "$kvargs{consumer}", "$kvargs{bridge}";
        open my $STDOLD, '>&', STDOUT;
        open STDOUT, '>', \$buff_str;     # Pretending a string is the stdout file
        while( <$in_stream> ) {
            print $out_stream "$_";       # It writes lines to the consumer application
        }
        open STDOUT, '>&', $STDOLD;
        close $out_stream;
    }

    {
        local $/;
        return <$buff_str>;
    }
}

use constant {
    CONSUME_APP => "cloud-modules/consumer-app/bin/Debug/net6.0/consumer-app",
};

sub fetch_secret {
    my $buff_str = shift;
    open my $IN_DATA, "<", \$buff_str;
    my $out_buffer = &_wrap_execution(
        *$IN_DATA,
        'consumer' => CONSUME_APP,
        'bridge'   => "BRIDGE_SECRET_ID_REQ",
    );
    close $IN_DATA;

    # It stands for the status returned by the last pipe close
    # This is just the 16-bit status word returned
    # by the traditional Unix wait() system call
    if ( ($? >> 8) != 0 ) {
        my $emsg = sprintf "%s\n", $out_buffer;
        print STDERR $emsg;
        exit $? >> 8;
    }

    return $out_buffer;
}


my $secret = fetch_secret "sheldon-cooper-says";
print $secret;
