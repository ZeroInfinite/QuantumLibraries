﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Xunit;
using Microsoft.Quantum.Chemistry;
using Microsoft.Quantum.Simulation.Core;

using System.Text.RegularExpressions;
using System;
using System.Linq;
using System.Collections.Generic;

namespace Microsoft.Quantum.Chemistry.Tests
{
    using FermionTerm = FermionTerm;
    using SpinOrbital = SpinOrbital;

     

    public class FermionTermTests
    {
        [Fact]
        public void EmptyFermioNTerm()
        {
            var term = new FermionTerm(new List<LadderOperator>());
            var term2 = new FermionTerm(new List<LadderOperator>());

            Dictionary<FermionTerm, double> dictionary = new Dictionary<FermionTerm, double>();
            dictionary.Add(term, 0.5);

            Assert.Equal(0.5, dictionary[term2]);
        }


        [Theory]
        [InlineData(3, new Int64[] { 0, 0, 0, 0 }, 1)]
        [InlineData(2, new Int64[] { 0, 1, 3, 0 }, 3)]
        [InlineData(2, new Int64[] { 0, 0, 1, 0 }, 2)]
        [InlineData(3, new Int64[] { 1, 2, 2, 1 }, 2)]
        [InlineData(3, new Int64[] { 4, 3, 2, 1 }, 4)]
        public void UniqueIndicesTests(Int64 norbitals, Int64[] idx, Int64 uniqueIndices)
        {
            var spinOrbitals = idx.Select(o => new SpinOrbital(norbitals, o)).ToInts(norbitals).Select(o => (int) o).ToList();
            var coefficient = 1.0;
            var fermionTerm = new FermionTerm(spinOrbitals);
            Assert.True(fermionTerm.GetUniqueIndices() == uniqueIndices);
        }

        [Theory]
        [InlineData(true, 10, new Int64[] { 0, 0, 0, 0 }, new Int64[] {0,0,0,0 })]
        [InlineData(true, 10, new Int64[] { 1, 1, 0, 0 }, new Int64[] { 0, 0, 0, 0 })]
        [InlineData(true, 10, new Int64[] { 1, 0, 0, 0 }, new Int64[] { 0, 0, 0, 0 })]
        [InlineData(true, 10, new Int64[] { 1, 1, 0, 0 }, new Int64[] { 1, 2, 2, 1 })]
        [InlineData(true, 10, new Int64[] { 0, 0, 0, 0 }, new Int64[] { 9, 2, 2, 1 })]
        [InlineData(false, 10, new Int64[] { 1, 1, 0, 0 }, new Int64[] { 5, 7, 6, 1 })]
        [InlineData(true, 10, new Int64[] { 1, 1, 1, 0 }, new Int64[] { 1, 19, 19, 3 })]
        [InlineData(true, 10, new Int64[] { 1, 0 }, new Int64[] { 1,5 })]
        [InlineData(true, 10, new Int64[] { 1, 0 }, new Int64[] { 1, 1 })]
        [InlineData(false, 10, new Int64[] { 0, 0, 0, 0 }, new Int64[] { 0, 0, 0, 1 })]
        [InlineData(false, 10, new Int64[] { 1, 1, 0, 0 }, new Int64[] { 0, 1, 1, 2 })]
        [InlineData(false, 10, new Int64[] { 1, 0, 0, 0 }, new Int64[] { 0, 5, 4, 5 })]
        [InlineData(false, 10, new Int64[] { 1, 1, 0, 0 }, new Int64[] { 3, 2, 2, 1 })]
        [InlineData(false, 10, new Int64[] { 0, 0, 0, 0 }, new Int64[] { 1, 2, 2, 3 })]
        [InlineData(false, 10, new Int64[] { 1, 1, 0, 0 }, new Int64[] { 5, 7, 6, 7 })]
        [InlineData(true, 10, new Int64[] { 1, 1, 1, 0 }, new Int64[] { 1, 9, 9, 12 })]
        [InlineData(true, 10, new Int64[] { 1, 1, 0, 0 }, new Int64[] { 0, 1, 2, 0 })]
        [InlineData(false, 10, new Int64[] { 1, 1, 0, 0 }, new Int64[] { 0, 2, 1, 0 })]
        [InlineData(false, 10, new Int64[] { 1, 0 }, new Int64[] { 6, 5 })]
        [InlineData(false, 10, new Int64[] { 1, 1 }, new Int64[] { 6, 5 })]
        public void IsInCanonicalOrderTest(bool pass, Int64 nOrbitals, Int64[] ca, Int64[] idx)
        {
            var ladderOperators = ca.Zip(idx, (a, b) => (a == 0 ? LadderOperator.Type.d : LadderOperator.Type.u, (int) b)).Select(o => new LadderOperator(o)).ToList();
            var tmp = new FermionTerm(ladderOperators);
            if (pass)
            {
                Assert.True(tmp.IsInCanonicalOrder());
            }
            else
            {
                Assert.False(tmp.IsInCanonicalOrder());
            }
        }

        [Theory]
        [InlineData(true, 10, new Int64[] { 0, 1 }, new Int64[] { 0, 1 })]
        [InlineData(true, 10, new Int64[] { 0, 1 }, new Int64[] { 1, 0 })]
        [InlineData(true, 10, new Int64[] { 0, 0, 1, 1 }, new Int64[] { 1, 2, 3, 4 })]
        public void ToCanonicalOrderNoNewTermsTest(bool pass, Int64 nOrbitals, IEnumerable<Int64> ca, IEnumerable<Int64> idx)
        {
            var ladderOperators = ca.Zip(idx, (a, b) => (a == 0 ? LadderOperator.Type.d : LadderOperator.Type.u, (int)b)).Select(o => new LadderOperator(o)).ToList();
            var tmp = new FermionTerm(ladderOperators);
            var newTerms = tmp.ToCanonicalOrder();
            foreach (var newTerm in newTerms)
            {
                Assert.True(newTerm.Item2.IsInCanonicalOrder());
            }

        }


        [Theory]
        [InlineData(true, 10, new Int64[] { 0, 1 }, new Int64[] { 1, 1 })]
        [InlineData(true, 10, new Int64[] { 1, 1, 0, 0 }, new Int64[] { 0, 1, 2, 0 })]
        [InlineData(true, 10, new Int64[] { 1, 1, 0, 0 }, new Int64[] { 0, 2, 1, 0 })]
        [InlineData(true, 10, new Int64[] { 0, 1, 1, 1 }, new Int64[] { 1, 1, 3, 4 })]
        [InlineData(true, 10, new Int64[] { 0, 1, 1 }, new Int64[] { 0, 0, 1 })]
        [InlineData(true, 10, new Int64[] { 0, 1, 1 }, new Int64[] { 0, 1, 0 })]
        public void ToCanonicalOrderTest(bool pass, Int64 nOrbitals, IEnumerable<Int64> ca, IEnumerable<Int64> idx)
        {
            var ladderOperators = ca.Zip(idx, (a, b) => (a == 0 ? LadderOperator.Type.d : LadderOperator.Type.u, (int)b)).Select(o => new LadderOperator(o)).ToList();
            var tmp = new FermionTerm(ladderOperators);
            var newTerms = tmp.ToCanonicalOrder();
            foreach (var newTerm in newTerms)
            {
                Assert.True(newTerm.Item2.IsInCanonicalOrder());
            }

        }
        /*
    [Fact]
    public void IsInCanonicalOrderCommonTypesTest()
    {
        var tmp = new FermionTermType[] { IdentityTermType,
        PPTermType,
        PQTermType,
        PQQPTermType,
        PQQRTermType,
        PQRSTermType};
        foreach (var item in tmp)
        {
            Assert.True(item.IsInCanonicalOrder());
        }
    }
    */
        /*
        [Theory]
        [InlineData(true, new Int64[] { 1, 2, 1, 3 }, new Spin[] { Spin.u, Spin.u, Spin.d, Spin.d }, -1.0)]
        [InlineData(true, new Int64[] { 1, 2, 1, 3 }, new Spin[] { Spin.d, Spin.u, Spin.d, Spin.d }, 1.0)]
        public void CreateFermionTermTest(bool pass, Int64[] orbitalIdx, Spin[] spinIdx, Double sign)
        {
            var coeff = 1.0;
            var spinOrbital = orbitalIdx.Zip(spinIdx, (a, b) => new SpinOrbital(a, b));
            var tmp = new FermionTerm((IEnumerable<SpinOrbital>)spinOrbital, (double)coeff);
            Assert.True(tmp.IsInCanonicalOrder());
            Assert.True(tmp.coeff == sign);
        }
        */
        /*
    [Theory]
    [InlineData(3, new Int64[] {}, 0)]
    [InlineData(3, new Int64[] { 0, 0 }, 1)]
    [InlineData(3, new Int64[] { 0, 1 }, 2)]
    [InlineData(2, new Int64[] { 0, 1, 3, 0 }, 4)]
    [InlineData(3, new Int64[] { 1, 2, 2, 1 }, 3)]
    [InlineData(3, new Int64[] { 4, 3, 2, 1 }, 5)]
    public void GetFermionTermTypeTests(Int64 norbitals, Int64[] idx, Int64 type)
    {
        var tmp = new FermionTermType[] {   IdentityTermType,
                                            PPTermType,
                                            PQTermType,
                                            PQQPTermType,
                                            PQQRTermType,
                                            PQRSTermType};


        var spinOrbitals = idx.Select(o => new SpinOrbital(norbitals, o)).ToArray();
        var coefficient = 1.0;
        var fermionTerm = new FermionTerm((SpinOrbital[])spinOrbitals, (double)coefficient);
        var fermionTermType = fermionTerm.GetFermionTermType();
        Assert.True(fermionTermType == tmp[type]);
    }
    */
    }

}