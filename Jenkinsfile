pipeline {
  agent any
  environment {
    DOTNET_CLI_TELEMETRY_OPTOUT = '1'
    BUILD_CONFIGURATION = 'Release'
    API_PROJECT_DIR = 'API_Automation'
    UI_PROJECT_DIR  = 'UI_Automation'
    WORKSPACE_ALLURE = "${env.WORKSPACE}/allure-results"
    ALLURE_REPORT_DIR = "${env.WORKSPACE}/allure-report"
  }

  stages {
    stage('Checkout') {
      steps {
        checkout scm
      }
    }

    stage('Clean previous results & report') {
      steps {
        script {
          if (isUnix()) {
            sh """
              rm -rf "${ALLURE_REPORT_DIR}" "${WORKSPACE_ALLURE}"
              rm -rf ${API_PROJECT_DIR}/bin/**/allure-results ${UI_PROJECT_DIR}/bin/**/allure-results || true
            """
          } else {
              bat """
              rmdir /s /q "%WORKSPACE%\\allure-report" 2>nul || echo.
              rmdir /s /q "%WORKSPACE%\\allure-results" 2>nul || echo.
              for /d %%D in ("%WORKSPACE%\\${API_PROJECT_DIR}\\bin\\*") do ^
                @rmdir /s /q "%%D\\allure-results" 2>nul || echo.
              for /d %%D in ("%WORKSPACE%\\${UI_PROJECT_DIR}\\bin\\*") do ^
                @rmdir /s /q "%%D\\allure-results" 2>nul || echo.
            """
          }
        }
      }
    }

    stage('Restore & Build') {
      steps {
        script {
            sh 'dotnet restore'
            sh "dotnet build -c ${BUILD_CONFIGURATION}"
        }
      }
    }

    stage('API Tests') {
      steps {
        script {
          // run API tests first
          if (isUnix()) {
            sh """
              dotnet test ${API_PROJECT_DIR} \
                -c ${BUILD_CONFIGURATION} \
                --no-build \
                --logger "console;verbosity=normal"
            """
          } else {
            bat "dotnet test ${API_PROJECT_DIR} -c %BUILD_CONFIGURATION% --no-build --logger \"console;verbosity=normal\""
          }
        }
      }
      post {
        always {
          script {
            // collect API allure results into workspace/allure-results
            if (isUnix()) {
              sh """
                mkdir -p "${WORKSPACE_ALLURE}/api" || true
                cp -r ${API_PROJECT_DIR}/bin/${BUILD_CONFIGURATION}/net8.0/allure-results/* "${WORKSPACE_ALLURE}/api/" 2>/dev/null || true
              """
            } else {
              bat """
                if not exist "%WORKSPACE%\\allure-results\\api" mkdir "%WORKSPACE%\\allure-results\\api"
                powershell -Command "Copy-Item -Path '${API_PROJECT_DIR}\\bin\\${BUILD_CONFIGURATION}\\net8.0\\allure-results\\*' -Destination '%WORKSPACE%\\allure-results\\api' -Recurse -Force -ErrorAction SilentlyContinue"
              """
            }
          }
        }
      }
    }

    stage('UI Tests') {
      steps {
        script {
          if (isUnix()) {
            sh "dotnet test ${UI_PROJECT_DIR} -c ${BUILD_CONFIGURATION} --no-build --logger \"console;verbosity=normal\""
          } else {
            bat "dotnet test ${UI_PROJECT_DIR} -c %BUILD_CONFIGURATION% --no-build --logger \"console;verbosity=normal\""
          }
        }
      }
      post {
        always {
          script {
            // collect UI allure results into workspace/allure-results
            if (isUnix()) {
              sh """
                mkdir -p "${WORKSPACE_ALLURE}/ui" || true
                cp -r ${UI_PROJECT_DIR}/bin/${BUILD_CONFIGURATION}/net8.0/allure-results/* "${WORKSPACE_ALLURE}/ui/" 2>/dev/null || true
              """
            } else {
              bat """
                if not exist "%WORKSPACE%\\allure-results\\ui" mkdir "%WORKSPACE%\\allure-results\\ui"
                powershell -Command "Copy-Item -Path '${UI_PROJECT_DIR}\\bin\\${BUILD_CONFIGURATION}\\net8.0\\allure-results\\*' -Destination '%WORKSPACE%\\allure-results\\ui' -Recurse -Force -ErrorAction SilentlyContinue"
              """
            }
          }
        }
      }
    }

    stage('Merge results & Generate Allure report') {
      steps {
        script {
          // merge: leave files as-is under workspace/allure-results (API and UI subfolders)
          // Allure CLI accepts a directory with multiple result files;
          // merge subfolders into a single input folder expected by allure CLI
          if (isUnix()) {
            sh """
              mkdir -p "${WORKSPACE_ALLURE}/merged"
              cp -r "${WORKSPACE_ALLURE}/api/"* "${WORKSPACE_ALLURE}/merged/" 2>/dev/null || true
              cp -r "${WORKSPACE_ALLURE}/ui/"* "${WORKSPACE_ALLURE}/merged/" 2>/dev/null || true

              # Generate report: note __allure generate__ cleans the output directory (it does NOT delete input results)
              allure generate "${WORKSPACE_ALLURE}/merged" -o "${ALLURE_REPORT_DIR}" --clean
            """
          } else {
            bat """
              if not exist "%WORKSPACE%\\allure-results\\merged" mkdir "%WORKSPACE%\\allure-results\\merged"
              powershell -Command "Copy-Item -Path '%WORKSPACE%\\allure-results\\api\\*' -Destination '%WORKSPACE%\\allure-results\\merged' -Recurse -Force -ErrorAction SilentlyContinue"
              powershell -Command "Copy-Item -Path '%WORKSPACE%\\allure-results\\ui\\*' -Destination '%WORKSPACE%\\allure-results\\merged' -Recurse -Force -ErrorAction SilentlyContinue"

              REM Generate report (requires 'allure' CLI on PATH)
              call allure generate "%WORKSPACE%\\allure-results\\merged" -o "%WORKSPACE%\\allure-report" --clean
            """
          }
        }
      }
    }
  }
}
