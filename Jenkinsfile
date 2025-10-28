pipeline {
  agent any

  environment {
    DOTNET_CLI_TELEMETRY_OPTOUT = '1'
    CONFIGURATION = 'Release'

    // Adjust these if your repo uses different paths/names
    API_PROJECT_DIR = 'API_Automation'        // path to .csproj or folder
    UI_PROJECT_DIR = 'UI_Automation'         // path to .csproj or folder

    // Allure paths
    WORKSPACE_ALLURE = "${env.WORKSPACE}/allure-results"
    ALLURE_REPORT_DIR = "${env.WORKSPACE}/allure-report"

    // We'll point Allure .NET adapter to write into WORKSPACE_ALLURE
    ALLURE_CONFIG = "${env.WORKSPACE}/AllureConfig.json"
  }

  options {
    timestamps()
    buildDiscarder(logRotator(numToKeepStr: '15'))
  }

  stages {
    stage('Checkout') {
      steps {
        // Works if the Jenkins job is connected to your GitHub repo;
        // otherwise replace with explicit 'git url: "...", branch: "main"'
        checkout scm
      }
    }

    stage('Prepare workspace') {
      steps {
        bat '''
          rem ---- clean old outputs (ignore if missing)
          if exist "%ALLURE_REPORT_DIR%" rmdir /s /q "%ALLURE_REPORT_DIR%"
          if exist "%WORKSPACE_ALLURE%"  rmdir /s /q "%WORKSPACE_ALLURE%"
          if exist "TestResults"         rmdir /s /q "TestResults"

          rem ---- recreate folders
          mkdir "%WORKSPACE_ALLURE%"
          mkdir "TestResults"

          rem ---- write AllureConfig.json at repo root
          > "%WORKSPACE%\\AllureConfig.json" (
            echo {
            echo   "allure": {
            echo     "directory": "allure-results"
            echo   }
            echo }
          )
        '''
      }
    }

    stage('Restore & Build (Docker)') {
      steps {
        bat '''
            docker run --rm ^
            -v "%WORKSPACE%":/src ^
            -w /src ^
            mcr.microsoft.com/dotnet/sdk:8.0 ^
            bash -lc "dotnet --info && \
                      dotnet restore ${API_PROJECT_DIR}/*.csproj && \
                      dotnet restore ${UI_PROJECT_DIR}/*.csproj && \
                      dotnet build ${API_PROJECT_DIR}/*.csproj -c ${CONFIGURATION} --no-restore && \
                      dotnet build ${UI_PROJECT_DIR}/*.csproj -c ${CONFIGURATION} --no-restore"
        '''
      }
    }

    stage('Test API Project (Docker)') {
      steps {
        bat '''
            docker run --rm ^
            -v "%WORKSPACE%":/src ^
            -w /src ^
            -e ALLURE_CONFIG=/src/AllureConfig.json ^
            mcr.microsoft.com/dotnet/sdk:8.0 ^
            bash -lc "mkdir -p /src/allure-results && \
                      dotnet test "${API_PROJECT_DIR}/*.csproj -c ${CONFIGURATION} --no-build \
                      --logger \\"trx;LogFileName=TestResults_API.trx\\""
        '''
      }
      post {
        always {
          // Keep TRX files for debugging even if tests fail
          bat 'mkdir -p TestResults && find . -name "*.trx" -exec cp {} TestResults/ \\; || true'
        }
      }
    }

    stage('Test UI Project (Docker)') {
      steps {
        bat '''
            docker run --rm ^
            -v "%WORKSPACE%":/src ^
            -w /src ^
            -e ALLURE_CONFIG=/src/AllureConfig.json ^
            mcr.microsoft.com/dotnet/sdk:8.0 ^
            bash -lc "mkdir -p /src/allure-results && \
                      dotnet test ${UI_PROJECT_DIR}/*.csproj -c ${CONFIGURATION} --no-build \
                      --logger \\"trx;LogFileName=TestResults_UI.trx\\""
        """
        '''
      }
      post { always { bat """for /r %%f in (*.trx) do copy /Y "%%f" "%WORKSPACE%\\TestResults\\" >nul""" } }
    }

    stage('Publish Allure Report') {
      steps {
        // Jenkins Allure Plugin pick ups generated results from WORKSPACE_ALLURE
        // If you named your Allure tool in Global Tool Configuration, you can pass it here.
        script {
          allure includeProperties: false,
                jdk: '',
                results: [[path: "${env.WORKSPACE}\\allure-results"]],
                reportBuildPolicy: 'ALWAYS'
        }
      }
    }
  }

  post {
    always {
      // Archive artifacts for later download (TRX + allure raw + optional report folder)
      archiveArtifacts artifacts: 'TestResults/**/*.trx, allure-results/**', fingerprint: true, allowEmptyArchive: true

    // If you also want to generate static HTML locally (without the plugin), uncomment:
    // bat '''
    //   if ! command -v allure >/dev/null 2>&1; then
    //     echo "Allure CLI not found in PATH. Skipping local generation (plugin will still publish)."
    //   else
    //     allure generate -c "${WORKSPACE_ALLURE}" -o "${ALLURE_REPORT_DIR}" || true
    //   fi
    // '''
    // archiveArtifacts artifacts: 'allure-report/**', fingerprint: true, allowEmptyArchive: true
    }
    failure {
      echo 'Build failed. Check TRX logs and Allure report for details.'
    }
  }
}
