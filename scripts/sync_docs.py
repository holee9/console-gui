#!/usr/bin/env python3
"""
MRD 기준 전체 문서 동기화 스크립트

MRD v3.0을 기준 소스(Single Source of Truth)로 사용하여
하위 문서의 메타데이터를 자동 동기화합니다.

사용법:
    python scripts/sync_docs.py              # 전체 동기화
    python scripts/sync_docs.py --check      # 검증만 (수정 안 함)
    python scripts/sync_docs.py --verbose    # 상세 로그

동기화 대상:
    1. 구 버전 참조 → 현행 버전으로 교체
    2. P1~P4 잔존 표기 → 경고 (자동 교체 불가)
    3. HnVue HnVue 중복 → HnVue
    4. RadiConsole → HnVue
    5. Mermaid 비flowchart classDef → 제거
"""

import re
import os
import sys
import json
from datetime import datetime

# ============================================================
# 설정
# ============================================================

DOCS_DIR = os.path.join(os.path.dirname(os.path.dirname(os.path.abspath(__file__))), 'docs')
ARCHIVE_DIR = os.path.join(DOCS_DIR, 'archive')

# 현행 버전 정의 (MRD 개정 시 여기만 업데이트)
CURRENT_VERSIONS = {
    'MRD': 'v3.0',
    'PRD': 'v2.0',
    'FRS': 'v2.0',
    'SRS': 'v2.0',
    'RTM': 'v2.0',
    'eSTAR': 'v2.0',
    'WBS': 'v1.0',
    'SAD': 'v2.0',
    'SDS': 'v2.0',
}

# 버전 교체 맵 (구 → 현행)
VERSION_REPLACEMENTS = {
    'PRD v3.0': f'PRD {CURRENT_VERSIONS["PRD"]}',
    'PRD v1.0': f'PRD {CURRENT_VERSIONS["PRD"]}',
    'WBS v4.0': f'WBS {CURRENT_VERSIONS["WBS"]}',
    'WBS v2.0': f'WBS {CURRENT_VERSIONS["WBS"]}',
    'WBS v3.0': f'WBS {CURRENT_VERSIONS["WBS"]}',
    'FRS v1.0': f'FRS {CURRENT_VERSIONS["FRS"]}',
    'SRS v1.0': f'SRS {CURRENT_VERSIONS["SRS"]}',
    'RTM v1.0': f'RTM {CURRENT_VERSIONS["RTM"]}',
    'eSTAR v1.0': f'eSTAR {CURRENT_VERSIONS["eSTAR"]}',
    # 파일 경로
    'DOC-004_FRS_v1.0': f'DOC-004_FRS_{CURRENT_VERSIONS["FRS"]}',
    'DOC-005_SRS_v1.0': f'DOC-005_SRS_{CURRENT_VERSIONS["SRS"]}',
    'DOC-032_RTM_v1.0': f'DOC-032_RTM_{CURRENT_VERSIONS["RTM"]}',
    'DOC-036_510k_eSTAR_v1.0': f'DOC-036_510k_eSTAR_{CURRENT_VERSIONS["eSTAR"]}',
}

# ============================================================
# 동기화 함수
# ============================================================

def sync_version_refs(content, filename):
    """구 버전 참조를 현행 버전으로 교체"""
    changes = []
    for old, new in VERSION_REPLACEMENTS.items():
        if old in content:
            count = content.count(old)
            content = content.replace(old, new)
            changes.append(f"  {old} -> {new} ({count}건)")
    return content, changes


def sync_product_name(content, filename):
    """제품명 통일"""
    changes = []
    
    # RadiConsole → HnVue
    radi_count = len(re.findall(r'RadiConsole', content, re.IGNORECASE))
    if radi_count:
        content = re.sub(r'RadiConsole\u2122', 'HnVue', content)
        content = re.sub(r'RadiConsole', 'HnVue', content, flags=re.IGNORECASE)
        changes.append(f"  RadiConsole -> HnVue ({radi_count}건)")
    
    # HnVue HnVue → HnVue
    dup_count = content.count('HnVue HnVue')
    if dup_count:
        content = content.replace('HnVue HnVue', 'HnVue')
        changes.append(f"  HnVue HnVue -> HnVue ({dup_count}건)")
    
    return content, changes


def sync_mermaid(content, filename):
    """Mermaid 차트 문제 수정"""
    changes = []
    
    def fix_block(match):
        block = match.group(0)
        lines = block.split('\n')
        first = ''
        for l in lines:
            s = l.strip()
            if s and not s.startswith('```'):
                first = s
                break
        
        # 비flowchart에서 classDef 제거
        if not any(kw in first for kw in ['flowchart', 'graph ', 'classDiagram']):
            new_lines = [l for l in lines if not l.strip().startswith('classDef')]
            if len(new_lines) < len(lines):
                changes.append(f"  classDef 제거 ({first[:30]})")
            return '\n'.join(new_lines)
        
        return block
    
    content = re.sub(r'```mermaid\n.*?```', fix_block, content, flags=re.DOTALL)
    return content, changes


def check_issues(content, filename):
    """수정 불가능한 이슈 경고"""
    warnings = []
    
    # P1~P4 잔존
    old_p = re.findall(r'☑ P[1-4]|☐ P[1-4]', content)
    if old_p:
        warnings.append(f"  ⚠️ P1~P4 잔존 {len(old_p)}건 (수동 개정 필요)")
    
    # Tier 미사용 (핵심 문서가 아닌 경우 경고만)
    if not re.search(r'Tier [1-4]', content):
        warnings.append(f"  ℹ️ Tier 표기 없음 (Phase별 개정 시 추가)")
    
    return warnings


# ============================================================
# 메인
# ============================================================

def main():
    check_only = '--check' in sys.argv
    verbose = '--verbose' in sys.argv
    
    mode = "검증 모드" if check_only else "동기화 모드"
    print(f"{'='*60}")
    print(f"MRD 기준 문서 동기화 ({mode})")
    print(f"기준: MRD {CURRENT_VERSIONS['MRD']}")
    print(f"시각: {datetime.now().strftime('%Y-%m-%d %H:%M')}")
    print(f"{'='*60}")
    
    total_files = 0
    total_changes = 0
    total_warnings = 0
    
    for root, dirs, files in os.walk(DOCS_DIR):
        if 'archive' in root:
            continue
        for f in files:
            if not f.endswith('.md'):
                continue
            
            path = os.path.join(root, f)
            with open(path, 'r') as fh:
                content = fh.read()
            
            original = content
            all_changes = []
            all_warnings = []
            
            # 동기화 실행
            content, changes = sync_version_refs(content, f)
            all_changes.extend(changes)
            
            content, changes = sync_product_name(content, f)
            all_changes.extend(changes)
            
            content, changes = sync_mermaid(content, f)
            all_changes.extend(changes)
            
            # 이슈 체크
            warnings = check_issues(content, f)
            all_warnings.extend(warnings)
            
            total_files += 1
            
            if all_changes or all_warnings:
                if all_changes:
                    total_changes += len(all_changes)
                    print(f"\n{'✏️' if not check_only else '🔍'} {f}")
                    for c in all_changes:
                        print(c)
                
                if all_warnings and verbose:
                    if not all_changes:
                        print(f"\n🔍 {f}")
                    for w in all_warnings:
                        print(w)
                    total_warnings += len(all_warnings)
                
                # 파일 저장 (check 모드가 아닌 경우)
                if not check_only and content != original:
                    with open(path, 'w') as fh:
                        fh.write(content)
    
    print(f"\n{'='*60}")
    print(f"결과: {total_files}개 파일 검사, {total_changes}건 {'수정' if not check_only else '발견'}")
    if verbose:
        print(f"경고: {total_warnings}건 (수동 개정 필요)")
    print(f"{'='*60}")


if __name__ == '__main__':
    main()
