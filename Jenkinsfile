pipeline {
  agent {
    // Ensures 'dotnet' is available even on bare agents
    docker {
      image 'mcr.microsoft.com/dotnet/sdk:8.0'
      // Run as root to allow temp installs if needed
      args '-u root:root -v $WORKSPACE:$WORKSPACE'
      reuseNode true
    }
  }

  environment {
    DOTNET_CLI_TELEMETRY_OPTOUT = '1'
    CONFIGURATION = 'Release'

    // Adjust these if your repo uses different paths/names
    API_PROJECT_DIR = 'API_Automation/API_Automation.csproj'        // path to .csproj or folder
    UI_PROJECT_DIR = 'UI_Automation/UI_Automation.csproj'         // path to .csproj or folder

    // Allure paths
    WORKSPACE_ALLURE = "${env.WORKSPACE}/allure-results"
    ALLURE_REPORT_DIR = "${env.WORKSPACE}/allure-report"

    // We'll point Allure .NET adapter to write into WORKSPACE_ALLURE
    ALLURE_CONFIG = "${env.WORKSPACE}/AllureConfig.json"
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
        sh '''
          rm -rf "${ALLURE_REPORT_DIR}" "${WORKSPACE_ALLURE}" TestResults || true
          mkdir -p "${WORKSPACE_ALLURE}" TestResults

          cat > "${ALLURE_CONFIG}" <<'JSON'
          {
            "allure": {
              "directory": "allure-results"
            }
          }
          JSON
        '''
      }
    }

    stage('Restore & Build') {
      steps {
        sh '''
          dotnet --info
          dotnet restore
          dotnet build -c "${CONFIGURATION}" --no-restore
        '''
      }
    }

    stage('Test API Project') {
      steps {
        sh '''
          # If API_PROJECT_DIR points to a folder containing a single .csproj, dotnet will pick it up.
          # Otherwise set it to the .csproj path, e.g., API_Automation/API_Automation.csproj
          dotnet test "${API_PROJECT_DIR}" \
            -c "${CONFIGURATION}" --no-build \
            --logger "trx;LogFileName=TestResults_API.trx"
        '''
      }
      post {
        always {
          // Keep TRX files for debugging even if tests fail
          sh 'mkdir -p TestResults && find . -name "*.trx" -exec cp {} TestResults/ \\; || true'
        }
      }
    }

    stage('Test UI Project') {
      steps {
        sh '''
          dotnet test "${UI_PROJECT_DIR}" \
            -c "${CONFIGURATION}" --no-build \
            --logger "trx;LogFileName=TestResults_UI.trx"
        '''
      }
      post {
        always {
          sh 'mkdir -p TestResults && find . -name "*.trx" -exec cp {} TestResults/ \\; || true'
        }
      }
    }

    stage('Publish Allure Report') {
      steps {
        // Jenkins Allure Plugin pick ups generated results from WORKSPACE_ALLURE
        // If you named your Allure tool in Global Tool Configuration, you can pass it here.
        script {
          allure includeProperties: false,
                 jdk: '',
                 results: [[path: "${env.WORKSPACE_ALLURE}"]],
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
    // sh '''
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
