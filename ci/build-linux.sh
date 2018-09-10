#!/bin/bash
schroot -c $RID -- VERSION=$VERSION; RID=$RID; echo Running on version $VERSION with rid $RID; ci/build.sh