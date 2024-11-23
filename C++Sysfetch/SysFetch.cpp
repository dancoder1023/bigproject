#include <iostream>
#include <string>
#include <fstream>
#include <sstream>
#include <cstdlib>
#include <sys/utsname.h>

std::string getOS() {
    std::ifstream file("/etc/os-release");
    std::string line;
    while (std::getline(file, line)) {
        if (line.find("PRETTY_NAME=") != std::string::npos) {
            return line.substr(line.find("=") + 1);
        }
    }
    return "Unknown Linux Distribution";
}

std::string getKernel() {
    struct utsname buffer;
    if (uname(&buffer) == 0) {
        return std::string(buffer.release);
    }
    return "Unknown";
}

std::string getCPU() {
    std::ifstream file("/proc/cpuinfo");
    std::string line, cpuModel;
    while (std::getline(file, line)) {
        if (line.find("model name") != std::string::npos) {
            cpuModel = line.substr(line.find(":") + 2);
            break;
        }
    }
    return cpuModel.empty() ? "Unknown" : cpuModel;
}

std::string getGPU() {
    std::string result;
    FILE* pipe = popen("lspci | grep -i 'vga\\|3d'", "r");
    if (!pipe) return "Unknown";
    char buffer[128];
    while (fgets(buffer, sizeof(buffer), pipe) != nullptr) {
        result += buffer;
    }
    pclose(pipe);
    return result.empty() ? "Unknown" : result;
}

std::string getPackages() {
    std::string result;
    FILE* pipe = popen("dpkg -l | wc -l", "r");
    if (!pipe) return "Unknown";
    char buffer[128];
    while (fgets(buffer, sizeof(buffer), pipe) != nullptr) {
        result += buffer;
    }
    pclose(pipe);
    return result.empty() ? "Unknown" : result;
}

void displayInfo() {
    std::cout << "\n";
    std::cout << "=====================================\n";
    std::cout << "           System Information         \n";
    std::cout << "=====================================\n";
    std::cout << "OS:          " << getOS() << "\n";
    std::cout << "Kernel:      " << getKernel() << "\n";
    std::cout << "CPU:         " << getCPU() << "\n";
    std::cout << "GPU:         " << getGPU() << "\n";
    std::cout << "Packages:    " << getPackages() << "\n";
    std::cout << "=====================================\n";
}

int main() {
    displayInfo();
    return 0;
}

