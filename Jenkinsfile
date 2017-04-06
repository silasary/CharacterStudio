node {
    stage('Clone') {
      checkout scm
	  }

    stage('Build') {
		  msbuild()
    }

	  stage('Publish'){
		  publish('CharacterStudio/bin/Release/CharacterStudio.exe')
		  publish('CharacterStudio-XP/bin/Release/CharacterStudio-XP.exe')
	  }

    stage('Archive') {
      archive '**/bin/**/'
      //archiveArtifacts allowEmptyArchive: false, artifacts: '\'**/bin/**/', caseSensitive: false, excludes: null, fingerprint: true, onlyIfSuccessful: true
    }

	  stage('Post-Build') {
	    step([$class: 'WarningsPublisher', canComputeNew: false, canResolveRelativePaths: false, consoleParsers: [[parserName: 'MSBuild']], defaultEncoding: '', excludePattern: '', healthy: '', includePattern: '', messagesPattern: '', unHealthy: ''])
	  }
}
