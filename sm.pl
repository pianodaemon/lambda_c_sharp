use strict;

sub _pipe_execution {
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


sub _wrap_execution {
    my $buff_str = shift;
    my $consumer_app = shift;
    my $consumer_bridge = shift;
    unless ( -e $consumer_app ) {
        my $emsg = sprintf "No presence of the consumer application %s\n", $consumer_app;
        print STDERR $emsg;
        exit 127;
    }
    open my $IN_DATA, "<", \$buff_str;
    my $out_buffer = &_pipe_execution(
        *$IN_DATA,
        'consumer' => $consumer_app,
        'bridge'   => $consumer_bridge,
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


sub fetch_secret {
    my $secret_id = shift;
    return &_wrap_execution(
        $secret_id,
        $ENV{'CONSUME_APP'},
        "BRIDGE_SECRET_ID_REQ"
    );
}

my $secret = fetch_secret "sheldon-cooper-says";
print $secret;
