#!/usr/bin/env sh
set -e

# Synopsis:
# Test the representer Docker image by running it against a predefined set of 
# solutions with an expected output.
# The representer Docker image is built automatically.

# Output:
# Outputs the diff of the expected representation files against the actual 
# representation files generated by the test runner Docker image.

# Example:
# ./bin/run-tests-in-docker.sh

# Build the Docker image
docker build --rm -t exercism/fsharp-representer .

# Run the Docker image using the settings mimicking the production environment
docker run \
    --rm \
    --network none \
    --read-only \
    --mount type=bind,src="${PWD}/tests",dst=/opt/representer/tests \
    --mount type=tmpfs,dst=/tmp \
    --volume "${PWD}/bin/run-tests.sh:/opt/representer/bin/run-tests.sh" \
    --workdir /opt/representer \
    --entrypoint /opt/representer/bin/run-tests.sh \
    exercism/fsharp-representer
