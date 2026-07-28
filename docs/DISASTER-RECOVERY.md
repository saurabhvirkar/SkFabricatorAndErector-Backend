# Disaster Recovery & Zero-State VM Reconstruction Protocol

## Overview
This runbook provides step-by-step instructions to rebuild the entire application server infrastructure from scratch in under **10 minutes** in the event of complete Oracle VM destruction or provider outage.

---

## 🛑 Disaster Recovery Time Objective (RTO) & Recovery Point Objective (RPO)
- **RTO (Recovery Time Objective)**: < 10 Minutes (Automated VM provisioning & container initialization).
- **RPO (Recovery Point Objective)**: Continuous (Neon Serverless PostgreSQL manages database state with point-in-time recovery; Cloudinary handles file persistence).

---

## Reconstruction Workflow

```
       Oracle VM Lost / Destroyed
                   │
                   ▼
      Step 1: Terraform Provisioning
   `cd deploy/terraform/oracle && terraform apply`
                   │
                   ▼
      Step 2: Ansible Host Configuration
      `ansible-playbook -i inventory deploy/ansible/site.yml`
                   │
                   ▼
      Step 3: Deploy Backend Containers
      `docker compose -f deploy/docker/docker-compose.prod.yml up -d`
                   │
                   ▼
      Step 4: Reconnect Neon PostgreSQL & Cloudinary
      (External state immediately re-attached via environment variables)
                   │
                   ▼
       APPLICATION RESTORED & HEALTHY
```

---

## Step-by-Step Recovery Procedure

### 1. Provision New VM Instance (Terraform)
Execute Terraform script to recreate Oracle Cloud Virtual Cloud Network, Subnet, Ingress rules, and Ubuntu Compute instance:
```bash
cd deploy/terraform/oracle
terraform init
terraform apply -auto-approve
```

### 2. Configure Host Security & Docker (Ansible)
Run Ansible playbook against the new VM's public IP to install Docker, Nginx, UFW Firewall, Fail2ban, and create non-root `deployer` user:
```bash
cd deploy/ansible
ansible-playbook -i "<new-vm-ip>," -u ubuntu site.yml
```

### 3. Deploy Containers & Reverse Proxy
SSH into the newly provisioned host and pull the latest production container images:
```bash
ssh deployer@<new-vm-ip>
cd /opt/skfabricator/prod
docker compose -f docker-compose.prod.yml pull
docker compose -f docker-compose.prod.yml up -d
```

### 4. Verify System Health
Execute health probe check:
```bash
curl -i https://api.skfabricator.com/health/ready
```
Expected output: `HTTP/1.1 200 OK` with `{"status":"Ready"}`.
