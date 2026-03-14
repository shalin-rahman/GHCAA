import { Component, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';

interface TierDetail {
    id: string;
    name: string;
    shortDesc: string;
    criteria: string[];
    rights: string[];
    color: string;
}

@Component({
    selector: 'landing-membership',
    standalone: true,
    imports: [CommonModule, RouterLink],
    templateUrl: './membership.html',
    styleUrl: './membership.scss',
})
export class LandingMembership {
    membershipTiers: TierDetail[] = [
        {
            id: 'founding',
            name: 'Founding Member',
            shortDesc: 'The pioneers of the GHCAA movement, providing initial vision and infrastructure.',
            color: 'var(--tier-founding)',
            criteria: [
                'Recognized for exceptional contribution during the setup phase.',
                'Must submit a "Declaration of Non-Political Engagement".',
                'Certified alumni with minimum 20 years post-HSC experience.',
                'Unanimously approved by the Steering Committee.'
            ],
            rights: [
                'In perpetuity voting rights.',
                'Permanent status in the Advisory Council.',
                'Priority seating at all constitutional events.'
            ]
        },
        {
            id: 'life',
            name: 'Life Member',
            shortDesc: 'Dedicated alumni committed to the lifelong support of Haraganga College.',
            color: '#B8860B',
            criteria: [
                'Full payment of Life Membership endowment fee.',
                'Certified alumni with minimum 10 years post-HSC experience.',
                'Professional bachelor degree or equivalent.'
            ],
            rights: [
                'Voter status for life without annual renewal.',
                'Access to "Elite Alumni" networking lounge and events.',
                'Exemption from regular annual administrative dues.'
            ]
        },
        {
            id: 'honorary',
            name: 'Honorary Member',
            shortDesc: 'Distinguished personalities who have brought glory to the institution.',
            color: '#FF4500',
            criteria: [
                'Not necessarily an alumni of the college.',
                'Exceptional national or international achievement in any field.',
                'Nominated by at least three Founding Members.'
            ],
            rights: [
                'Non-voting observational status.',
                'Guest of Honor at major annual reunions.',
                'Right to provide honorary mentorship to graduating students.'
            ]
        },
        {
            id: 'executive',
            name: 'Executive Member',
            shortDesc: 'Active leadership category for contributing to association operations.',
            color: 'var(--tier-executive)',
            criteria: [
                'Certified alumni with minimum 10 years post-HSC experience.',
                'Bachelor Certified from a recognized institution.',
                'Proven record of active role in events and decision-making.',
                'Requires payment of annual administrative fee.'
            ],
            rights: [
                'Full voting rights during the 3-year term.',
                'Eligible for specific EC roles (Vice President, Secretary, etc.).',
                'Right to lead sub-committees and special event projects.'
            ]
        },
        {
            id: 'general',
            name: 'General Member',
            shortDesc: 'The primary voting category for all qualified alumni worldwide.',
            color: 'var(--tier-general)',
            criteria: [
                'Certified alumni with minimum 5 years post-HSC experience.',
                'Payment of one-time registration and regular annual dues.',
                'Maintain good moral standing and disciplinary record.'
            ],
            rights: [
                'Voting rights after 5 years of continuous membership.',
                'Eligible for EC elections as General Member representatives.',
                'Access to the professional member directory and job hub.'
            ]
        },
        {
            id: 'associate',
            name: 'Associate Member',
            shortDesc: 'Inclusive category for those who shared the Haraganga journey.',
            color: 'var(--tier-associate)',
            criteria: [
                'Open to any verified ex-student (min 1 year of study).',
                'No minimum years since graduation required.',
                'One-time registration fee payment only.'
            ],
            rights: [
                'Non-voting membership category.',
                'Access to standard networking events and open programs.',
                'Eligibility to upgrade to General Membership upon satisfying criteria.'
            ]
        },
        {
            id: 'advisory',
            name: 'Advisory Member',
            shortDesc: 'Distinguished cohort providing high-level institutional guidance.',
            color: 'var(--tier-advisory)',
            criteria: [
                'Current Principal or Vice Principal of the College (Ex-officio).',
                'Former Principals or Teachers of GHC.',
                'Exceptional alumni with min. 10 years of demonstrable community service.'
            ],
            rights: [
                'Non-voting (unless specifically granted by EC).',
                'May attend strategic/AGM meetings upon invitation.',
                'Act as a bridge between alumni, college, and stakeholders.'
            ]
        }
    ];

    selectedTier = signal<TierDetail>(this.membershipTiers[0]);
}

